import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit, AfterViewInit {
  model: any = {};

  @ViewChild('loginForm', { static: false }) loginForm: NgForm;

  constructor(public authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
  }

  login() {
    if (this.loginForm.valid) {
      this.authService.login(this.model)
        .subscribe((next) => {
          this.alertify.success('Logged in successfully!');
        }, (error) => {
          this.alertify.error(error);
        });
    }
  }

  loggedIn() {
    // const token = localStorage.getItem('token');
    // return !!token;
    return this.authService.loggedIn();
  }

  logout() {
    localStorage.removeItem('token');
    this.alertify.message('logged out');
  }

}
