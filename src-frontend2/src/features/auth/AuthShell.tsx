import type { ReactNode } from 'react';
import ThemeToggle from '../../components/ui/ThemeToggle';
import LanguageToggle from '../../components/layout/LanguageToggle';

interface AuthShellProps {
  subtitle: string;
  children: ReactNode;
}

export default function AuthShell({ subtitle, children }: AuthShellProps) {
  return (
    <div className="relative flex min-h-screen items-center justify-center bg-bg px-4 py-10">
      <div className="absolute right-4 top-4 flex gap-2">
        <ThemeToggle />
        <LanguageToggle />
      </div>
      <div className="w-full max-w-md">
        <div className="mb-8 text-center">
          <h1 className="bg-gradient-to-r from-amber-400 to-amber-600 bg-clip-text text-4xl font-extrabold tracking-tight text-transparent">
            🐝 BeeZillion
          </h1>
          <p className="mt-2 text-muted">{subtitle}</p>
        </div>
        {children}
      </div>
    </div>
  );
}
