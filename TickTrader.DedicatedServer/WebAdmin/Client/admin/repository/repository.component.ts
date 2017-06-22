import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus, ObservableRequest, ResponseCode } from '../../models/index';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')]
})

export class RepositoryComponent implements OnInit {
    public UploadRequest: ObservableRequest<void>;
    public CheckPackageRequest: ObservableRequest<void>;
    public SelectedFile: any;
    public Packages: PackageModel[];

    @ViewChild('PackageInput')
    PackageInput: any;

    constructor(private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this.UploadRequest = null;
        this.CheckPackageRequest = null;

        this._api.Feed.AddOrUpdatePackage.subscribe(algoPackage => this.addOrUpdatePackage(algoPackage));
        this._api.Feed.DeletePackage.subscribe(pname => this.deletePackage(pname));

        this.loadPackages();
    }

    public get SelectedFileName() {
        if (this.SelectedFile)
            return this.SelectedFile.name;
        else
            return "";
    }

    public OnPackageDeleted(algoPackage: PackageModel) {
        this.deletePackage(algoPackage.Name);
    }

    public InitUploadPackage() {
        this.CheckPackageRequest = new ObservableRequest(this._api.PackageExists(this.SelectedFileName))
            .Subscribe(ok => { },
            err => {
                if (err.Status !== 404) {
                    this.CheckPackageRequest = null;
                    this._toastr.error(err.Message);
                }
                else if (err.Status === 404)
                    this.UploadPackage();
            });
    }

    public UploadPackage() {
        this.CheckPackageRequest = null;

        this.UploadRequest = new ObservableRequest(this._api.UploadPackage(this.SelectedFile))
            .Subscribe(ok => {
                this.SelectedFile = null;
                this.PackageInput.nativeElement.value = "";
            },
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                }
            })
    }

    CancelUploadingPackage() {
        this.CheckPackageRequest = null;
    }

    public OnFileInputChange(event) {
        this.UploadRequest = null;
        this.CheckPackageRequest = null;
        this.SelectedFile = event.target.files[0];
    }

    public get CanUpload() {
        return !!this.SelectedFile && (!this.UploadRequest || this.UploadRequest && this.UploadRequest.IsCompleted);
    }

    public Cancel() {
        this.UploadRequest = null;
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
