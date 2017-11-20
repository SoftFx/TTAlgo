import {Injectable, Inject} from '@angular/core';
import { Resource, GuiEn } from '../resx/index';


@Injectable()
export class ResourceService {
	private _currentLang: string;
    private readonly resources: any;

	public get CurrentLang() {
	  return this._currentLang;
	}

	constructor() {
        this._currentLang = GuiEn.name;
        this.resources = { [GuiEn.name]: GuiEn.dictionary }
	}

	public Use(lang: string): void {
		this._currentLang = lang;
	}

    private pop(key: string): string {
        if (this.Contains(key)) {
			return this.resources[this.CurrentLang][key];
		}

		return key;
	}

    public Contains(key: string) {
        return this.resources[this.CurrentLang] && this.resources[this.CurrentLang][key];
    }

	public Instant(key: string) {
		return this.pop(key); 
	}
}