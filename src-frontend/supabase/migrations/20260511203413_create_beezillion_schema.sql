/*
  # Create BeeZillion Schema

  1. New Tables
    - `profiles`
      - `id` (uuid, primary key, references auth.users)
      - `email` (text, unique)
      - `streak` (integer, default 0)
      - `daily_goal` (integer, default 10)
      - `last_activity` (timestamptz)
      - `created_at` (timestamptz)
    - `words`
      - `id` (uuid, primary key)
      - `user_id` (uuid, references profiles)
      - `original` (text)
      - `translation` (text)
      - `ai_sentence` (text, nullable)
      - `interval_days` (float, default 1)
      - `ease_factor` (float, default 2.5)
      - `repetitions` (integer, default 0)
      - `next_review_at` (timestamptz)
      - `created_at` (timestamptz)
    - `reviews`
      - `id` (uuid, primary key)
      - `word_id` (uuid, references words)
      - `user_id` (uuid, references profiles)
      - `outcome` (integer)
      - `reviewed_at` (timestamptz)
    - `badges`
      - `id` (uuid, primary key)
      - `user_id` (uuid, references profiles)
      - `type` (integer)
      - `awarded_at` (timestamptz)

  2. Security
    - Enable RLS on all tables
    - Users can only access their own data
    - Service role has full access for edge functions

  3. Notes
    - Streak and daily goal are stored in profiles
    - Words use SM-2 spaced repetition fields (interval_days, ease_factor, repetitions)
    - Reviews track each review event for statistics
*/

-- Create profiles table
CREATE TABLE IF NOT EXISTS profiles (
  id uuid PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  email text UNIQUE NOT NULL,
  streak integer DEFAULT 0,
  daily_goal integer DEFAULT 10,
  last_activity timestamptz DEFAULT now(),
  created_at timestamptz DEFAULT now()
);

-- Create words table
CREATE TABLE IF NOT EXISTS words (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  original text NOT NULL,
  translation text NOT NULL,
  ai_sentence text,
  interval_days double precision DEFAULT 1,
  ease_factor double precision DEFAULT 2.5,
  repetitions integer DEFAULT 0,
  next_review_at timestamptz DEFAULT now(),
  created_at timestamptz DEFAULT now()
);

-- Create reviews table
CREATE TABLE IF NOT EXISTS reviews (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  word_id uuid NOT NULL REFERENCES words(id) ON DELETE CASCADE,
  user_id uuid NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  outcome integer NOT NULL,
  reviewed_at timestamptz DEFAULT now()
);

-- Create badges table
CREATE TABLE IF NOT EXISTS badges (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id uuid NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  type integer NOT NULL,
  awarded_at timestamptz DEFAULT now()
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_words_user_id ON words(user_id);
CREATE INDEX IF NOT EXISTS idx_words_next_review ON words(user_id, next_review_at);
CREATE INDEX IF NOT EXISTS idx_reviews_user_id ON reviews(user_id);
CREATE INDEX IF NOT EXISTS idx_reviews_word_id ON reviews(word_id);
CREATE INDEX IF NOT EXISTS idx_badges_user_id ON badges(user_id);

-- Enable RLS
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE words ENABLE ROW LEVEL SECURITY;
ALTER TABLE reviews ENABLE ROW LEVEL SECURITY;
ALTER TABLE badges ENABLE ROW LEVEL SECURITY;

-- Profiles policies
CREATE POLICY "Users can read own profile"
  ON profiles FOR SELECT
  TO authenticated
  USING (auth.uid() = id);

CREATE POLICY "Users can update own profile"
  ON profiles FOR UPDATE
  TO authenticated
  USING (auth.uid() = id)
  WITH CHECK (auth.uid() = id);

-- Words policies
CREATE POLICY "Users can read own words"
  ON words FOR SELECT
  TO authenticated
  USING (auth.uid() = user_id);

CREATE POLICY "Users can insert own words"
  ON words FOR INSERT
  TO authenticated
  WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can update own words"
  ON words FOR UPDATE
  TO authenticated
  USING (auth.uid() = user_id)
  WITH CHECK (auth.uid() = user_id);

CREATE POLICY "Users can delete own words"
  ON words FOR DELETE
  TO authenticated
  USING (auth.uid() = user_id);

-- Reviews policies
CREATE POLICY "Users can read own reviews"
  ON reviews FOR SELECT
  TO authenticated
  USING (auth.uid() = user_id);

CREATE POLICY "Users can insert own reviews"
  ON reviews FOR INSERT
  TO authenticated
  WITH CHECK (auth.uid() = user_id);

-- Badges policies
CREATE POLICY "Users can read own badges"
  ON badges FOR SELECT
  TO authenticated
  USING (auth.uid() = user_id);

-- Function to auto-create profile on signup
CREATE OR REPLACE FUNCTION public.handle_new_user()
RETURNS trigger
LANGUAGE plpgsql
SECURITY DEFINER
AS $$
BEGIN
  INSERT INTO public.profiles (id, email)
  VALUES (NEW.id, NEW.email);
  RETURN NEW;
END;
$$;

-- Trigger for auto-creating profile
DROP TRIGGER IF EXISTS on_auth_user_created ON auth.users;
CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW
  EXECUTE FUNCTION public.handle_new_user();

