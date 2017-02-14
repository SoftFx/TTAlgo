import { Component, OnInit, OnDestroy } from '@angular/core';
import { Http, Request, Response, RequestOptionsArgs, Headers } from '@angular/http';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../app.component.css')],
})

export class RepositoryComponent implements OnInit {
    public selectedFile: any = { name: '' };
    public packages: any[];

    constructor(private http: Http) {

    }

    ngOnInit() {
        this.loadPackages();
    }

    public loadPackages() {
        this.http.get("/api/Repository")
            .map(res => res.json())
            .subscribe(res => {
                console.log(res);
                this.packages = res
            });
    }

    public uploadFile() {
        let input = new FormData();
        input.append("file", this.selectedFile);

        this.http.post("/api/Repository", input)
            .subscribe(res => {
                console.log(res);
                this.loadPackages();
            });
    }


    public onFileInputChange(event) {
        this.selectedFile = event.srcElement.files[0];
    }
}
