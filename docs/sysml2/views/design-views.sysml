// Views rendered into docs/design/generated/ for the Design document.
// Reopens the ReqStream package so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package ReqStream {
    view SoftwareStructureView {
        expose ReqStream;
    }

    view ReqStreamView {
        expose ReqStreamSystem;
    }

    view CliView {
        expose CliSubsystem;
    }

    view ModelingView {
        expose ModelingSubsystem;
    }

    view TracingView {
        expose TracingSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }

    view UtilitiesView {
        expose UtilitiesSubsystem;
    }
}
