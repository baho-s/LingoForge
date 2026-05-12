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
    const path = url.pathname.replace("/functions/v1/dashboard", "") || "/";

    if (path === "/" && req.method === "GET") {
      const [profileRes, badgesRes, wordsRes, reviewsTodayRes] = await Promise.all([
        supabase.from("profiles").select("streak, daily_goal, last_activity").eq("id", user.id).maybeSingle(),
        supabase.from("badges").select("type, awarded_at").eq("user_id", user.id).order("awarded_at", { ascending: false }),
        supabase.from("words").select("created_at").eq("user_id", user.id).order("created_at", { ascending: false }).limit(100),
        supabase.from("reviews").select("id").eq("user_id", user.id).gte("reviewed_at", new Date().toISOString().split("T")[0]),
      ]);

      const profile = profileRes.data;
      const badges = badgesRes.data || [];
      const wordsReviewedToday = (reviewsTodayRes.data || []).length;

      // Build weekly activity from word creation dates
      const now = new Date();
      const weeklyActivity: Array<{ date: string; wordsAdded: number }> = [];
      for (let i = 6; i >= 0; i--) {
        const d = new Date(now.getTime() - i * 24 * 60 * 60 * 1000);
        const dateStr = d.toISOString().split("T")[0];
        const count = (wordsRes.data || []).filter((w: { created_at: string }) =>
          w.created_at.startsWith(dateStr)
        ).length;
        weeklyActivity.push({ date: dateStr, wordsAdded: count });
      }

      const dashboard = {
        streak: profile?.streak ?? 0,
        dailyGoal: profile?.daily_goal ?? 10,
        lastActivity: profile?.last_activity ?? new Date().toISOString(),
        wordsReviewedToday,
        badges: badges.map((b: { type: number; awarded_at: string }) => ({
          type: b.type,
          awardedAt: b.awarded_at,
        })),
        weeklyActivity,
      };

      return new Response(JSON.stringify(dashboard), {
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
