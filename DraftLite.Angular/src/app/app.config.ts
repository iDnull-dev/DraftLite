import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Nora from '@primeng/themes/nora';
import { definePreset } from '@primeng/themes';

import { routes } from './app.routes';

// ─── DraftLite Theme Preset ───────────────────────────────────────────────
// Extends Nora with Soho dark surfaces and Purple primary.
// ─────────────────────────────────────────────────────────────────────────

const DraftLitePreset = definePreset(Nora, {
  semantic: {
    primary: {
      50:  '#f5f3ff',
      100: '#ede9fe',
      200: '#ddd6fe',
      300: '#c4b5fd',
      400: '#a78bfa',
      500: '#8b5cf6',
      600: '#7c3aed',
      700: '#6d28d9',
      800: '#5b21b6',
      900: '#4c1d95',
      950: '#3b0764',
    },
    colorScheme: {
      dark: {
        surface: {
          0:   '#ffffff',
          50:  '#fafafa',
          100: '#f4f4f5',
          200: '#e4e4e7',
          300: '#d4d4d8',
          400: '#a1a1aa',
          500: '#71717a',
          600: '#52525b',
          700: '#3f3f46',
          800: '#2a2a2d',
          // 850: '#232326',
          900: '#1c1c1e',
          950: '#0f0f10',
        },
      },
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    // provideZoneChangeDetection({ eventCoalescing: true }),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset:  DraftLitePreset,
        options: {
          darkModeSelector: 'body',   // force dark mode globally
          cssLayer: false,
        },
      },
    }),
  ]
};