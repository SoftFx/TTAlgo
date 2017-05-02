import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'bot-detail-cmp',
    template: require('./bot-detail.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotDetailComponent implements OnInit {

    ngOnInit() {

    }
}