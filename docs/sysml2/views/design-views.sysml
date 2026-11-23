// Views rendered into docs/design/generated/ for the Design document.
// Imports the ReqStream system facade so each 'expose' statement can reference
// its target part def directly. Per SysML2Tools syntax, 'expose' is only valid
// inside a named 'view' usage (not a 'view def' definition), and it - not
// 'render' - is what scopes the rendered diagram content.
package ReqStreamDesignViews {
    private import StandardViewDefinitions::*;
    private import ReqStream::*;

    view SoftwareStructureView : InterconnectionView {
        expose ReqStream::ReqStreamSystem;
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
