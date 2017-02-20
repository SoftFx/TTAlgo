import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { PackageModel, PluginModel, ResponseStatus } from '../../models/index';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')],
})

export class RepositoryComponent implements OnInit {
    public selectedFile: any;
    public packages: PackageModel[];
    public uploading: boolean;
    public uploadStatus: ResponseStatus;
    public deleteStatus: ResponseStatus;

    constructor(private api: ApiService) {

    }

    ngOnInit() {
        this.loadPackages();
    }

    private loadPackages() {
        this.api.getAlgoPackages()
            .subscribe(res => {
                this.packages = res;
            });
    }

    public deletePackage(algoPackage: PackageModel) {
        this.api
            .deleteAlgoPackage(algoPackage.name)
            .subscribe(() => {
                this.packages = this.packages.filter(p => p !== algoPackage);
            })
    }

    public uploadPackage() {
        this.uploadStatus = null;

        this.api
            .uploadAlgoPackage(this.selectedFile)
            .subscribe(res => {
                this.selectedFile = null;
                this.loadPackages();
            },
            err => {
                this.uploadStatus = err.json() as ResponseStatus;
            });
    }

    public onFileInputChange(event) {
        this.selectedFile = event.srcElement.files[0];
        this.uploadStatus = null;
    }

    public get selectedFileName(): string {
        if (this.selectedFile)
            return this.selectedFile.name;
        else
            return '';
    }

    public get canUpload(): boolean {
        return this.selectedFileName &&
            !this.packages.find(p => p.name == this.selectedFileName)
    }
}
