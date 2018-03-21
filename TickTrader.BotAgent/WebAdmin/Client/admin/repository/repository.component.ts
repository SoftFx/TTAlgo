import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus, ResponseCode } from '../../models/index';

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

        this.loadPackages();
    }

    public OnPackageDeleted(algoPackage: PackageModel) {
        this.deletePackage(algoPackage.Name);
    }

    private loadPackages() {
        this._api.GetPackages()
            .subscribe(res => {
                if (!this.Packages)
                    this.Packages = res
                else
                    res.forEach(p => this.addOrUpdatePackage(p));
            });
    }

    private addOrUpdatePackage(packageModel: PackageModel) {
        let index = -1;
        let fpackage = this.Packages.find((p, i) => { index = i; return p.Name === packageModel.Name });

        if (fpackage)
            this.Packages[index] = packageModel;
        else
            this.Packages = this.Packages.concat(packageModel);
    }

    private deletePackage(packageName: string) {
        this.Packages = this.Packages.filter(p => p.Name !== packageName);
    }
}
