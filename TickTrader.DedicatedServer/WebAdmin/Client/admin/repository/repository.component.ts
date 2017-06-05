import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus, ObservableRequest } from '../../models/index';
import { ViewChild } from '@angular/core';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')]
})

export class RepositoryComponent implements OnInit {
    public UploadRequest: ObservableRequest<void>;
    public SelectedFile: any;
    public Packages: PackageModel[];
    public Uploading: boolean = false;


    @ViewChild('PackageInput')
    PackageInput: any;

    constructor(private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this.UploadRequest = null;

        this._api.Feed.AddPackage.subscribe(algoPackage => this.addPackage(algoPackage));
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

    public UploadPackage() {
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

    public OnFileInputChange(event) {
        this.UploadRequest = null;
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
                    res.forEach(p => this.addPackage(p));
            });
    }

    private addPackage(packageModel: PackageModel) {
        if (!this.Packages.find(p => p.Name === packageModel.Name)) {
            this.Packages = this.Packages.concat(packageModel);
        }
    }

    private deletePackage(packageName: string) {
        this.Packages = this.Packages.filter(p => p.Name !== packageName);
    }

}
