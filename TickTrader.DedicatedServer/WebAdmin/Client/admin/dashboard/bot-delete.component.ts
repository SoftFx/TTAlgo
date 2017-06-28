import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { TradeBotModel, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'bot-delete-cmp',
    template: require('./bot-delete.component.html'),
    styles: [require('../../app.component.css')],
})

export class BotDeleteComponent implements OnInit {
    public CleanLog: boolean;
    public CleanAlgoData: boolean;
    public DeleteRequest: ObservableRequest<void>;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Bot: TradeBotModel;
    @Output() OnDeleted = new EventEmitter<TradeBotModel>();
    @Output() OnCanceled = new EventEmitter<void>();


    ngOnInit() {
        this.DeleteRequest = null;
        this.CleanAlgoData = true;
        this.CleanLog = true;
    }

    public Delete(cleanLog: boolean, cleanAlgoData: boolean) {
        this.DeleteRequest = new ObservableRequest(this._api.DeleteBot(this.Bot.Id, cleanLog, cleanAlgoData))
            .Subscribe(ok => this.OnDeleted.emit(this.Bot),
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.DeleteRequest = null;
                }
            })
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}
