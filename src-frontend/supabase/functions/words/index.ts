import { createClient } from "npm:@supabase/supabase-js@2";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type, Authorization, X-Client-Info, Apikey",
};

function sm2(outcome: number, intervalDays: number, easeFactor: number, repetitions: number) {
  let ef = easeFactor;
  let rep = repetitions;
  let interval = intervalDays;

  if (outcome < 3) {
    rep = 0;
    interval = 1;
  } else {
    rep += 1;
    if (rep === 1) interval = 1;
    else if (rep === 2) interval = 6;
    else interval = Math.round(interval * ef);
  }

  ef = Math.max(1.3, ef + (0.1 - (5 - outcome) * (0.08 + (5 - outcome) * 0.02)));

  const nextReview = new Date();
  nextReview.setDate(nextReview.getDate() + interval);

  return { intervalDays: interval, easeFactor: ef, repetitions: rep, nextReviewAt: nextReview.toISOString() };
}

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
    const path = url.pathname.replace("/functions/v1/words", "") || "/";

    // GET / - list all words
    if (path === "/" && req.method === "GET") {
      const { data, error } = await supabase
        .from("words")
        .select("*")
        .eq("user_id", user.id)
        .order("created_at", { ascending: false });

      if (error) {
        return new Response(JSON.stringify({ error: error.message }), {
          status: 500,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      const words = data.map((w: Record<string, unknown>) => ({
        id: w.id,
        original: w.original,
        translation: w.translation,
        aiSentence: w.ai_sentence,
        intervalDays: w.interval_days,
        easeFactor: w.ease_factor,
        repetitions: w.repetitions,
        nextReviewAt: w.next_review_at,
        createdAt: w.created_at,
      }));

      return new Response(JSON.stringify(words), {
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    // GET /word-of-day
    if (path === "/word-of-day" && req.method === "GET") {
      const { data, error } = await supabase
        .from("words")
        .select("*")
        .eq("user_id", user.id)
        .order("created_at", { ascending: false })
        .limit(1)
        .maybeSingle();

      if (error || !data) {
        return new Response(JSON.stringify({ error: "No words found" }), {
          status: 404,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      const word = {
        id: data.id,
        original: data.original,
        translation: data.translation,
        aiSentence: data.ai_sentence,
        intervalDays: data.interval_days,
        easeFactor: data.ease_factor,
        repetitions: data.repetitions,
        nextReviewAt: data.next_review_at,
        createdAt: data.created_at,
      };

      return new Response(JSON.stringify(word), {
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    // POST / - add word
    if (path === "/" && req.method === "POST") {
      const body = await req.json();
      const { original, translation, generateSentenceImmediately } = body;

      const aiSentence = generateSentenceImmediately
        ? `The word "${original}" means "${translation}" and can be used in many contexts.`
        : null;

      const { data, error } = await supabase
        .from("words")
        .insert({
          user_id: user.id,
          original,
          translation,
          ai_sentence: aiSentence,
          next_review_at: new Date().toISOString(),
        })
        .select()
        .single();

      if (error) {
        return new Response(JSON.stringify({ error: error.message }), {
          status: 500,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      const word = {
        id: data.id,
        original: data.original,
        translation: data.translation,
        aiSentence: data.ai_sentence,
        intervalDays: data.interval_days,
        easeFactor: data.ease_factor,
        repetitions: data.repetitions,
        nextReviewAt: data.next_review_at,
        createdAt: data.created_at,
      };

      return new Response(JSON.stringify(word), {
        status: 201,
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    // POST /bulk-generate
    if (path === "/bulk-generate" && req.method === "POST") {
      const { data: wordsWithoutSentence, error: fetchError } = await supabase
        .from("words")
        .select("id, original, translation")
        .eq("user_id", user.id)
        .is("ai_sentence", null);

      if (fetchError) {
        return new Response(JSON.stringify({ error: fetchError.message }), {
          status: 500,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      let generated = 0;
      let skipped = 0;

      for (const word of (wordsWithoutSentence || [])) {
        const sentence = `The word "${word.original}" means "${word.translation}" and can be used in many contexts.`;
        const { error: updateError } = await supabase
          .from("words")
          .update({ ai_sentence: sentence })
          .eq("id", word.id);

        if (updateError) skipped++;
        else generated++;
      }

      return new Response(JSON.stringify({ generated, skipped }), {
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    // POST /{id}/review
    const reviewMatch = path.match(/^\/([0-9a-f-]+)\/review$/);
    if (reviewMatch && req.method === "POST") {
      const wordId = reviewMatch[1];
      const body = await req.json();
      const outcome = body.outcome;

      const { data: word, error: wordError } = await supabase
        .from("words")
        .select("*")
        .eq("id", wordId)
        .eq("user_id", user.id)
        .maybeSingle();

      if (wordError || !word) {
        return new Response(JSON.stringify({ error: "Word not found" }), {
          status: 404,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      const result = sm2(outcome, word.interval_days, word.ease_factor, word.repetitions);

      await supabase
        .from("words")
        .update({
          interval_days: result.intervalDays,
          ease_factor: result.easeFactor,
          repetitions: result.repetitions,
          next_review_at: result.nextReviewAt,
        })
        .eq("id", wordId);

      await supabase.from("reviews").insert({
        word_id: wordId,
        user_id: user.id,
        outcome,
      });

      return new Response(JSON.stringify({ success: true }), {
        headers: { ...corsHeaders, "Content-Type": "application/json" },
      });
    }

    // DELETE /{id}
    const deleteMatch = path.match(/^\/([0-9a-f-]+)$/);
    if (deleteMatch && req.method === "DELETE") {
      const wordId = deleteMatch[1];
      const { error: deleteError } = await supabase
        .from("words")
        .delete()
        .eq("id", wordId)
        .eq("user_id", user.id);

      if (deleteError) {
        return new Response(JSON.stringify({ error: deleteError.message }), {
          status: 500,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      return new Response(JSON.stringify({ success: true }), {
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
