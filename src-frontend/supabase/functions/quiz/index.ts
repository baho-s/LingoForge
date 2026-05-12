import { createClient } from "npm:@supabase/supabase-js@2";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Methods": "GET, POST, PUT, DELETE, OPTIONS",
  "Access-Control-Allow-Headers": "Content-Type, Authorization, X-Client-Info, Apikey",
};

function shuffle<T>(arr: T[]): T[] {
  const a = [...arr];
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
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
    const path = url.pathname.replace("/functions/v1/quiz", "") || "/";

    if (path === "/" && req.method === "GET") {
      const mode = parseInt(url.searchParams.get("mode") || "0");
      const count = parseInt(url.searchParams.get("count") || "10");

      const { data: allWords, error: wordsError } = await supabase
        .from("words")
        .select("id, original, translation")
        .eq("user_id", user.id);

      if (wordsError) {
        return new Response(JSON.stringify({ error: wordsError.message }), {
          status: 500,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      if (!allWords || allWords.length < 2) {
        return new Response(JSON.stringify({ error: "Need at least 2 words to create a quiz" }), {
          status: 400,
          headers: { ...corsHeaders, "Content-Type": "application/json" },
        });
      }

      const selected = shuffle(allWords).slice(0, Math.min(count, allWords.length));

      const quizWords = selected.map((word: { id: string; original: string; translation: string }) => {
        const wrongAnswers = shuffle(
          allWords
            .filter((w: { id: string }) => w.id !== word.id)
            .map((w: { translation: string }) => w.translation)
        ).slice(0, 3);

        const isOriginalToTranslation = mode === 0 || mode === 1;
        const prompt = isOriginalToTranslation ? word.original : word.translation;
        const answer = isOriginalToTranslation ? word.translation : word.original;

        const options = shuffle([
          { value: answer, isCorrect: true },
          ...wrongAnswers.map((v: string) => ({ value: v, isCorrect: false })),
        ]);

        return {
          wordId: word.id,
          prompt,
          answer,
          options,
        };
      });

      return new Response(JSON.stringify(quizWords), {
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
