import { Component, ElementRef, HostListener, signal, ViewChild } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { Login } from '../login/login';
import { Register } from '../register/register';

declare var $: any;

@Component({
  selector: 'app-nav-menu',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, Login, Register],
  templateUrl: './nav-menu.html',
  styleUrl: './nav-menu.css',
})
export class NavMenu {
  isDesktopOpen = signal(false);
  isMobileOpen = signal(false);
  isLoginModalOpen = signal(false);
  isRegisterModalOpen = signal(false);
  isDrawerOpen = signal(false);

  toggleMenu(event: MouseEvent) {
    event.stopPropagation();     
    this.isDesktopOpen.update(state => !state);
  }

  toggleDrawer() {
    this.isDrawerOpen.set(!this.isDrawerOpen());
  }

  closeDrawer() {
    this.isDrawerOpen.set(!this.isDrawerOpen());
  }

  toggleDropdown(event: MouseEvent) {
      event.stopPropagation();     
      this.isDesktopOpen.update(state => !state);
  }

  toggleMobileDropdown(event: MouseEvent) {
      event.stopPropagation();     
      this.isMobileOpen.update(state => !state);
  }

  loginHandler(event: MouseEvent) {
    event.stopPropagation();
    this.isLoginModalOpen.update(state => !state);
  }

  registerHandler(event: MouseEvent) {
    event.stopPropagation();
    this.isRegisterModalOpen.update(state => !state);
  }


  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.relative')) {
      //this.isMobileOpen.set(false);    
      // this.isDrawerOpen.set(!this.isDrawerOpen());
      
      // this.isDesktopOpen.set(false);    
    }

  }


}
