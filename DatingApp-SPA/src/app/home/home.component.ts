import { Component, OnInit, Output } from '@angular/core';
import { EventEmitter } from 'protractor';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  registerMode = false;

  constructor() { }

  ngOnInit() {
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

}
