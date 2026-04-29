import { Routes } from '@angular/router';
import { AuthPageComponent } from './pages/auth/auth.page';
import { ConnectedPageComponent } from './pages/connected/connected.page';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth' },
  { path: 'auth', component: AuthPageComponent },
  { path: 'app', component: ConnectedPageComponent, canActivate: [AuthGuard] },
];
