import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SideNavStore {
  readonly isSidebarVisible = signal<boolean>(false);

  setIsSidebarVisible(isSidebarVisible: boolean) {
   this.isSidebarVisible.set(isSidebarVisible);
  }

  toggleSidebar(): void {
    this.setIsSidebarVisible(!this.isSidebarVisible());
  }
}

