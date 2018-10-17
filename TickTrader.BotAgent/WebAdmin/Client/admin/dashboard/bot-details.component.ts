import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable } from "rxjs/Rx";
import { ApiService, ToastrService } from '../../services/index';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TradeBotModel, TradeBotLog, ObservableRequest, TradeBotStates, TradeBotStateModel, ResponseStatus, LogEntryTypes, TradeBotStatus, FileInfo, WebUtility, ConnectionStatus } from '../../models/index';
import { setTimeout } from 'timers';

@Component({
    selector: 'bot-details-cmp',
    template: require('./bot-details.component.html'),
    styles: [require('../../app.component.css')]
})

export class BotDetailsComponent implements OnInit, OnDestroy {
    public ConfirmDeletionEnabled: boolean;
    public TradeBotState = TradeBotStates;
    public LogEntryType = LogEntryTypes;
    public BotId: string;
    public Bot: TradeBotModel;
    public Log: TradeBotLog;
    public AlgoData: FileInfo[];
    public Status: string;

    public BotRequest: ObservableRequest<TradeBotModel>;
    public LogRequest: ObservableRequest<TradeBotLog>;
    public AlgoDataRequest: ObservableRequest<FileInfo[]>;
    public StatusRequest: ObservableRequest<TradeBotStatus>;

    private isAliveFlag: boolean;

    constructor(
        private _route: ActivatedRoute,
        private _api: ApiService,
        private _router: Router,
        private _toastr: ToastrService
    ) { }

    ngOnInit() {
        this._route.params
            .subscribe((params: Params) => {
                this.BotId = params['id'];
                if (!this.BotId) {
                    this.BotRequest = new ObservableRequest(Observable.of(<TradeBotModel>null));
                    this.LogRequest = new ObservableRequest(Observable.of(<TradeBotLog>null));
                    this.AlgoDataRequest = new ObservableRequest(Observable.of(<FileInfo[]>null));
                    this.StatusRequest = new ObservableRequest(Observable.of(<TradeBotStatus>null));
                }
                else {
                    this.BotRequest = new ObservableRequest(this._api.GetTradeBot(this.BotId))
                    this.LogRequest = new ObservableRequest(this._api.GetTradeBotLog(this.BotId))
                    this.AlgoDataRequest = new ObservableRequest(this._api.GetTradeBotAlgoData(this.BotId))
                    this.StatusRequest = new ObservableRequest(this._api.GetTradeBotStatus(this.BotId))
                    this._api.Feed.ChangeBotState
                        .filter(state => this.Bot && this.Bot.Id == state.Id)
                        .subscribe(botState => this.updateBotState(botState));
                    this._api.Feed.ConnectionState.subscribe(state => { if (state == ConnectionStatus.Connected) this.getBotInfo(); });

                    this.isAliveFlag = true;
                    this.getBotLog();
                    this.getBotStatus();
                }

                this.BotRequest.Subscribe(res => this.Bot = res);
                this.LogRequest.Subscribe(res => this.Log = res);
                this.AlgoDataRequest.Subscribe(res => this.AlgoData = res);
                this.StatusRequest.Subscribe(res => this.Status = res ? res.Status : "");
            });
    }

    ngOnDestroy() {
        this.isAliveFlag = false;
    }

    public InitDeletion() {
        this.ConfirmDeletionEnabled = true;
    }

    public DeletionCanceled() {
        this.ConfirmDeletionEnabled = false;
    }

    public DeletionCompleted(tradeBot: TradeBotModel) {
        this._router.navigate(["/dashboard"]);
    }

    public DonwloadAlgoDataLink(botId: string, file: string) {
        return this._api.GetDownloadAlgoDataUrl(botId, file);
    }

    public DonwloadLogLink(botId: string, file: string) {
        return this._api.GetDownloadLogUrl(botId, file);
    }

    public get IsOnline(): boolean {
        return this.Bot.State === TradeBotStates.Running;
    }

    public get IsProcessing(): boolean {
        return this.Bot.State === TradeBotStates.Starting
            || this.Bot.State === TradeBotStates.Reconnecting
            || this.Bot.State === TradeBotStates.Stopping;
    }

    public get IsOffline(): boolean {
        return this.Bot.State === TradeBotStates.Stopped;
    }

    public get Faulted(): boolean {
        return this.Bot.State === TradeBotStates.Faulted;
    }

    public get Broken(): boolean {
        return this.Bot.State === TradeBotStates.Broken;
    }

    public get CanStop(): boolean {
        return (this.Bot.State === TradeBotStates.Running
            || this.Bot.State === TradeBotStates.Starting
            || this.Bot.State === TradeBotStates.Reconnecting) && !this.Broken;
    }

    public get CanStart(): boolean {
        return (this.Bot.State === TradeBotStates.Stopped
            || this.Bot.State === TradeBotStates.Faulted) && !this.Broken;
    }

    public get CanDelete(): boolean {
        return this.Bot.State === TradeBotStates.Stopped
            || this.Bot.State === TradeBotStates.Faulted
            || this.Broken;
    }

    public get CanConfigurate(): boolean {
        return (this.Bot.State === TradeBotStates.Stopped
            || this.Bot.State === TradeBotStates.Faulted) && !this.Broken;
    }

    public Start(botId: string) {
        this.Bot = <TradeBotModel>{ ...this.Bot, State: TradeBotStates.Starting }

        this._api.StartBot(botId).subscribe(
            ok => { },
            err => this.notifyAboutError(err)
        );
    }

    public Stop(botId: string) {
        this._api.StopBot(botId).subscribe(
            ok => { },
            err => this.notifyAboutError(err)
        );
    }

    public DeleteLogFile(botId: string, file: string) {
        this._api.DeleteLogFile(botId, file).subscribe(
            ok => this.Log.Files = this.Log.Files.filter(f => f.Name !== file),
            err => this.notifyAboutError(err)
        );
    }

    public DeleteAlgoDataFile(botId: string, file: string) {

    }

    public Configurate(botId: string) {
        if (botId)
            this._router.navigate(['/configurate', WebUtility.EncodeURIComponent(botId)]);
    }

    private getBotInfo() {
        this._api.GetTradeBot(this.BotId).subscribe(res => this.Bot = res);
        this._api.GetTradeBotAlgoData(this.BotId).subscribe(res => this.AlgoData = res)
    }

    private getBotLog() {
        if (!this.isAliveFlag)
            return;

        if (this._api.Feed.CurrentState === ConnectionStatus.Connected && this.Bot && !this.IsOffline) {
            this._api.GetTradeBotLog(this.BotId).subscribe(res => this.Log = res);
        }
        setTimeout(() => this.getBotLog(), 5000);
    }

    private getBotStatus() {
        if (!this.isAliveFlag)
            return;

        if (this._api.Feed.CurrentState === ConnectionStatus.Connected && this.Bot && !this.IsOffline) {
            this._api.GetTradeBotStatus(this.BotId).subscribe(res => this.Status = res ? res.Status : "");
        }
        setTimeout(() => this.getBotStatus(), 2000);
    }

    private updateBotState(botState: TradeBotStateModel) {
        this.Bot = <TradeBotModel>{ ...this.Bot, State: botState.State, FaultMessage: botState.FaultMessage }
    }

    private notifyAboutError(response: ResponseStatus) {
        this._toastr.error(response.Message);
    }

}