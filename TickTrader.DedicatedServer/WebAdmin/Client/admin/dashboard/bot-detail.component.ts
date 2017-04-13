import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ExtBotModel, BotModel, BotState, ParameterType } from '../../models/index';
import { ApiService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';

@Component({
    selector: 'bot-detail-cmp',
    template: require('./bot-detail.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotDetailComponent implements OnInit {
    parameterType = ParameterType;
    botStateType = BotState;

    bot: ExtBotModel;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private api: ApiService
    ) { }

    ngOnInit() {
        //this.route.params
        //    .switchMap((params: Params) => this.api.getBot(params['id']))
        //    .subscribe((bot: ExtBotModel) => this.bot = bot);
    }

    run() {
    }

    stop() {
    }

    configurate() {

    }

    remove() {
        this.router.navigate(["/dasboard"]);
    }
}