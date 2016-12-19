import { Component, OnInit } from '@angular/core';
import { FakeData, BotModel } from '../../models/index';

@Component({
    selector: 'bots-repository',
    template: require('./bots-repository.component.html'),
    styles: [require('../app/app.component.css')],
})
export class BotsRepositoryComponent implements OnInit {
    bots: BotModel[];

    ngOnInit() {
        this.bots = FakeData.bots;
    }
}