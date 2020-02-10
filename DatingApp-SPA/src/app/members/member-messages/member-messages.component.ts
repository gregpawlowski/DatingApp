import { Component, OnInit, Input } from '@angular/core';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { Message } from 'src/app/_models/message';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.scss']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService, private authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMesssages();
  }

  loadMesssages() {
    const currentUserId = +this.authService.decodedToken.nameid;

    this.userService.getMessageThread(currentUserId, this.recipientId)
      .pipe(
        tap( messages => {
          messages.forEach(message => {
            if (!message.isRead && message.recipientId === currentUserId) { this.userService.markAsRead(currentUserId, message.id); }
          });
        })
      )
      .subscribe(messages => this.messages = messages, error => this.alertify.error(error));
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe((message: Message) => {
        this.messages = [ message, ...this.messages ];
        this.newMessage.content = '';
      }, error => {
        this.alertify.error(error);
      });
  }

}
