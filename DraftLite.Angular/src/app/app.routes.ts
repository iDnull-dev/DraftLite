import { Routes } from '@angular/router';
import { AuthPageComponent } from './pages/auth/auth.page';
import { ConnectedPageComponent } from './pages/connected/connected.page';
import { AuthGuard } from './guards/auth.guard';
import { Home } from './pages/home/home.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth' },
  { path: 'auth', component: AuthPageComponent },
  { path: 'app', component: Home, canActivate: [AuthGuard] },
];
