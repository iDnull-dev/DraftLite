import { Routes } from '@angular/router';
import { ConnectedPageComponent } from './pages/connected/connected.page';
import { AuthGuard } from './guards/auth.guard';
import { Home } from './pages/home/home.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: Home},
];
