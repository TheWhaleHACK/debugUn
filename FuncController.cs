using System.Collections;
using System.Collections.Generic;
using Unigine;

using UnigineApp.data.Code.Auxiliary;

[Component(PropertyGuid = "ad7a67d3e168f91a2a624ec3eb471fdd93cd96af")]
public class FuncController : Component
{
    EventConnections connections = new EventConnections();


    [ShowInEditor]
    [ParameterFile(Filter = ".ui")]
    private string functionsUiPath;

    public static WidgetTabBox tabBox = null;
    public static WidgetHBox funcMain = null;
    public static WidgetHBox funcViews = null;
    public static WidgetHBox funcAnimation = null;
    public static WidgetHBox funcAnnotation = null;
    public static WidgetHBox funcVisualization = null;

    private UserInterface functionsUi = null;

    void Init()
    {
        EngineWindowViewport functions = WindowManager.GetWindow(2) as EngineWindowViewport;
        functionsUi = new UserInterface(functions.SelfGui, functionsUiPath);

        tabBox = functionsUi.GetWidget(functionsUi.FindWidget("tabBox")) as WidgetTabBox;

        WidgetVBox vbox = functionsUi.GetWidget(functionsUi.FindWidget("vbox")) as WidgetVBox;
        functions.AddChild(vbox, Gui.ALIGN_EXPAND);

        funcMain = functionsUi.GetWidget(functionsUi.FindWidget("main")) as WidgetHBox;
        funcMain.Height = functions.Gui.Height;

        funcViews = functionsUi.GetWidget(functionsUi.FindWidget("views")) as WidgetHBox;
        funcViews.Height = functions.Gui.Height;

        funcVisualization = functionsUi.GetWidget(functionsUi.FindWidget("visualization")) as WidgetHBox;
        funcVisualization.Height = functions.Gui.Height;

        funcAnnotation = functionsUi.GetWidget(functionsUi.FindWidget("annotation")) as WidgetHBox;
        funcAnnotation.Height = functions.Gui.Height;

        funcAnimation = functionsUi.GetWidget(functionsUi.FindWidget("animation")) as WidgetHBox;
        funcAnimation.Height = functions.Gui.Height;

        vbox.SetPosition(0, 0);


        // WidgetVBox vbox = new WidgetVBox(functions.Gui);
        // vbox.Background = 1;
        // vbox.SetPosition(0, 40);
        // functions.AddChild(vbox, Gui.ALIGN_OVERLAP | Gui.ALIGN_EXPAND);
        // vbox.Width = 200;

        // WidgetTabBox tabBox = new WidgetTabBox();
        // vbox.AddChild(tabBox, Gui.ALIGN_EXPAND);

        // WidgetButton button = new();
        // button.Text = "Button";
        // funcViews.AddChild(button, Gui.ALIGN_TOP);
    }

    void Update()
    {
        // write here code to be called before updating each render frame

    }
}