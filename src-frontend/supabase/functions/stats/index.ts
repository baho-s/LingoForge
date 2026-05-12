import { createClient } from "npm:@supabase/supabase-js@2";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type, Authorization, X-Client-Info, Apikey",
};

Deno.serve(async (req: Request) => {
  if (req.method === "OPTIONS") {
    return new Response(null, { status: 200, headers: corsHeaders });
  }

  try {
    const supabase = createClient(
      Deno.env.get("SUPABASE_URL")!,
      Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!
    );

    const authHeader = req.headers.get("Authorization");
    if (!authHeader) {
      return new Response(JSON.stringify({ error: "Unauthorized" }), {
        status: 401,
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    const token = authHeader.replace("Bearer ", "");
    const { data: { user }, error: authError } = await supabase.auth.getUser(token);
    if (authError || !user) {
      return new Response(JSON.stringify({ error: "Unauthorized" }), {
        status: 401,
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    const url = new URL(req.url);
    const path = url.pathname.replace("/functions/v1/stats", "") || "/";

    if (path === "/" && req.method === "GET") {
      const [wordsRes, reviewsRes] = await Promise.all([
        supabase.from("words").select("id, ai_sentence, next_review_at, ease_factor, repetitions").eq("user_id", user.id),
        supabase.from("reviews").select("outcome, reviewed_at").eq("user_id", user.id).order("reviewed_at", { ascending: false }).limit(500),
      ]);

      const words = wordsRes.data || [];
      const reviews = reviewsRes.data || [];
      const now = new Date();

      const wordsDueToday = words.filter((w: { next_review_at: string }) =>
        new Date(w.next_review_at) <= now
      ).length;
      const wordsWithAi = words.filter((w: { ai_sentence: string | null }) =>
        w.ai_sentence !== null
      ).length;
      const masteredWords = words.filter((w: { ease_factor: number; repetitions: number }) =>
        w.ease_factor >= 2.5 && w.repetitions >= 5
      ).length;

      // Calculate accuracy from reviews
      const correctReviews = reviews.filter((r: { outcome: number }) => r.outcome >= 3).length;
      const accuracy = reviews.length > 0 ? Math.round((correctReviews / reviews.length) * 100) : 0;

      // Reviews this week
      const weekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      const reviewsThisWeek = reviews.filter((r: { reviewed_at: string }) =>
        new Date(r.reviewed_at) >= weekAgo
      ).length;

      // Build review trend (last 7 days)
      const reviewTrend: Array<{ date: string; reviews: number; correct: number }> = [];
      for (let i = 6; i >= 0; i--) {
        const d = new Date(now.getTime() - i * 24 * 60 * 60 * 1000);
        const dateStr = d.toISOString().split("T")[0];
        const dayReviews = reviews.filter((r: { reviewed_at: string }) =>
          r.reviewed_at.startsWith(dateStr)
        );
        const dayCorrect = dayReviews.filter((r: { outcome: number }) => r.outcome >= 3).length;
        reviewTrend.push({ date: dateStr, reviews: dayReviews.length, correct: dayCorrect });
      }

      const stats = {
        totalWords: words.length,
        wordsDueToday,
        wordsWithAiSentence: wordsWithAi,
        wordsWithoutAiSentence: words.length - wordsWithAi,
        totalReviews: reviews.length,
        masteredWords,
        accuracy,
        reviewsThisWeek,
        reviewTrend,
      };

      return new Response(JSON.stringify(stats), {
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    return new Response(JSON.stringify({ error: "Not found" }), {
      status: 404,
      headers: { ...corsHeaders, "Content-Type": "application/json" },
    });
  } catch (err) {
    return new Response(JSON.stringify({ error: String(err) }), {
      status: 500,
      headers: { ...corsHeaders, "Content-Type": "application/json" },
    });
  }
});
