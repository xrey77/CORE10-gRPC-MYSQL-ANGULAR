import { AfterViewInit, Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

declare var $: any;

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login  {
  isModalOpen = signal(false);
  private router = inject(Router);

  // @ViewChild('staticModal') modalElement!: ElementRef<HTMLDialogElement>;
  submitLogin(event: MouseEvent) {
    event?.stopPropagation();
    alert("submit");
  }

  cancelLogin(event: MouseEvent) {
    event?.stopPropagation();
    // this.isModalOpen.set(false);
    location.reload();
    this.router.navigate(['/']); 
  }
  // ngAfterViewInit() {



  //   $('#cancelModalBtn').on('click', () => {
  //     this.isModalOpen.set(false);
  //     this.modalElement.nativeElement.close();
  //   });

  //   $('#confirmModalBtn').on('click', () => {
  //     this.isModalOpen.set(false);
  //     this.modalElement.nativeElement.close('confirmed');
  //   });
  // }


}
