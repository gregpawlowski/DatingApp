import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit, AfterViewInit {
  model: any = {};

  @ViewChild('loginForm', { static: false }) loginForm: NgForm;

  constructor(private authService: AuthService) { }

  ngOnInit() {
    console.log(this.loginForm);
  }

  ngAfterViewInit() {
    console.log('after view', this.loginForm);
  }

  login() {
    if (this.loginForm.valid) {
      this.authService.login(this.model)
        .subscribe((next) => {
          console.log('Logged in successfully');
        }, (error) => {
          console.log(error);
        });
    }
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token;
  }

  logout() {
    localStorage.removeItem('token');
    console.log('Logged out');
  }

}
