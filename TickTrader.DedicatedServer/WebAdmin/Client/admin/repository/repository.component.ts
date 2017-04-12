import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus } from '../../models/index';
import { ViewChild } from '@angular/core';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')]
})

export class RepositoryComponent implements OnInit {
    private _isFileDuplicated: boolean = false;
    private _uploadingError: ResponseStatus;

    public SelectedFile: any;
    public Packages: PackageModel[] = [];
    public Uploading: boolean = false;


    @ViewChild('PackageInput')
    PackageInput: any;

    constructor(private _api: ApiService, private _toastr: ToastrService) {
    }

    ngOnInit() {
        this._api.Feed.addPackage.subscribe(algoPackage => this.addPackage(algoPackage));
        this._api.Feed.deletePackage.subscribe(pname => this.deletePackage(pname));

        this.loadPackages();
    }

    public get SelectedFileName() {
        if (this.SelectedFile)
            return this.SelectedFile.name;
        else
            return "";
    }

    public OnPackageDeleted(algoPackage: PackageModel) {
        //this.deletePackage(algoPackage.Name);
    }

    public UploadPackage() {
        this._uploadingError = null;
        this.Uploading = true;

        this._api
            .UploadPackage(this.SelectedFile)
            .finally(() => { this.Uploading = false; })
            .subscribe(res => {
                this.SelectedFile = null;
                this.PackageInput.nativeElement.value = "";
            },
            err => {
                this._uploadingError = err;
                if (!this._uploadingError.Handled)
                    this._toastr.error(this._uploadingError.Message);
            });
    }

    public OnFileInputChange(event) {
        this._uploadingError = null;
        this.SelectedFile = event.target.files[0];
    }

    public get FileInputError() {
        if ((this._uploadingError != null && this._uploadingError['Code'] && this._uploadingError.Code == 1000) || this.isFileDuplicated) {
            return 'DuplicatePackage';
        }
        return null;
    }

    public get IsFileNameVaild(): boolean {
        let a = this.SelectedFile && !this.SelectedFile.name && !this.isFileDuplicated;
        return a;
    }

    public get CanUpload() {
        return !this.Uploading
            && this.SelectedFileName
            && !this.isFileDuplicated
            && (!this._uploadingError || !this._uploadingError['Code'] || this._uploadingError['Code'] != 1000);
    }

    private get isFileDuplicated(): boolean {
        return this.Packages.find(p => this.SelectedFile && p.Name == this.SelectedFile.name) != null;
    }

    private loadPackages() {
        this._api.GetPackages()
            .subscribe(res => this.Packages = res);
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
