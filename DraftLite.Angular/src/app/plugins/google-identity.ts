export type GoogleCredentialResponse = {
  credential: string;
};

declare global {
  interface Window {
    google?: {
      accounts?: {
        id?: {
          initialize: (config: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
            auto_select?: boolean;
            ux_mode?: string;
            context?: string;
          }) => void;
          renderButton: (parent: HTMLElement, options?: Record<string, unknown>) => void;
          prompt: (notification?: unknown) => void;
        };
      };
    };
  }
}

export function loadGoogleIdentityService(): Promise<void> {
  if (typeof window === 'undefined') return Promise.resolve();
  if (window.google?.accounts?.id) return Promise.resolve();

  return new Promise((resolve, reject) => {
    const existing = document.querySelector<HTMLScriptElement>(
      'script[data-draftlite="google-identity"]',
    );
    if (existing) {
      existing.addEventListener('load', () => resolve());
      existing.addEventListener('error', () => reject(new Error('Google identity script failed')));
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.dataset['draftlite'] = 'google-identity';
    script.addEventListener('load', () => resolve());
    script.addEventListener('error', () => reject(new Error('Google identity script failed')));
    document.head.appendChild(script);
  });
}

