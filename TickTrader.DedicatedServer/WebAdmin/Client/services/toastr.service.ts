import { Injectable } from '@angular/core';
import 'bootstrap-notify';
declare var $: any;

@Injectable()
export class ToastrService {

    public info(message: string) {
        this.notify(message, "info");
    }

    public success(message: string) {
        this.notify(message, "success");
    }

    public warning(message: string) {
        this.notify(message, "warning");
    }

    public error(message: string) {
        this.notify(message, "danger");
    }

    private notify(message: string, type: string)
    {
        var localToastSettings = Object.assign({}, this.toastSettings);
        localToastSettings.type = type;

        $.notify({
            message: message
        }, localToastSettings);
    }

    private toastSettings = {
        type: "",
        timer: 4000,
        allow_dismiss: true,
        newest_on_top: true,
        animate: {
            enter: 'animated fadeInDown',
            exit: 'animated fadeOutUp'
        },
        icon_type: 'class',
        placement: {
            from: 'top',
            align: 'right'
        }
    }
}



enum ToastrTypes {
    info,
    success,
    warning,
    danger
}

//template:
                //'<div data-notify="container" class="col-xs-11 col-sm-3 alert alert-{0}" role="alert">' +
                //'   <button type="button" aria-hidden="true" class="close" data-notify="dismiss">×</button>' +
                //'   <span data-notify="message">{2}</span>' +
                //'</div>'