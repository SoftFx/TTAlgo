import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus, ResponseCode, ConnectionStatus } from '../../models/index';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')]
})

export class RepositoryComponent implements OnInit {
    public Packages: PackageModel[];

    constructor(private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this._api.Feed.AddOrUpdatePackage.subscribe(algoPackage => this.addOrUpdatePackage(algoPackage));
        this._api.Feed.DeletePackage.subscribe(pname => this.deletePackage(pname));
        this._api.Feed.ConnectionState.subscribe(state => { if (state == ConnectionStatus.Connected) this.loadPackages(); });

        this.loadPackages();
    }

    public OnPackageDeleted(algoPackage: PackageModel) {
        this.deletePackage(algoPackage.Id);
    }

    private loadPackages() {
        this._api.GetPackages()
            .subscribe(res => this.Packages = res);
    }

    private addOrUpdatePackage(packageModel: PackageModel) {
        let index = -1;
        let fpackage = this.Packages.find((p, i) => { index = i; return p.Id === packageModel.Id });

        if (fpackage)
            this.Packages[index] = packageModel;
        else
            this.Packages = this.Packages.concat(packageModel);
    }

    private deletePackage(packageId: string) {
        this.Packages = this.Packages.filter(p => p.Id !== packageId);
    }
}
