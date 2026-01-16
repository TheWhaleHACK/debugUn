using System.Collections;
using System.Collections.Generic;
using Unigine;
using UnigineApp.data.Code;
using UnigineApp.data.Code.Auxiliary;

[Component(PropertyGuid = "a381a22c813efc4a5191cc9cec8c8a4dc85c8adc")]
public class CrossSectionUi : Component
{
    [ShowInEditor]
    public Node CUBE;

    // [ShowInEditor]
    // public AnimationGUI animationGUI;

    // [ShowInEditor]
    // private WindowController windowController = null;

    private EngineWindowViewport mainWindow;
    private Node currentNode;

    public static List<CrossSection> crossSections;
    public static int countCross;
    public static int indexCross;

    public static WidgetComboBox crossSectionType;
    private WidgetVBox functionsCrossVBox;

    static public WidgetCheckBox fillCheckBox;
    public static WidgetCheckBox showCrossCheckBox;
    public static WidgetCheckBox showPlaneCheckBox;

    private WidgetLabel labelCrossSection;
    private WidgetButton addCrossButton;
    private WidgetButton delCrossButton;


    private bool isCreatingCrossSection;
    private bool stopCreatingCrossSection;
    private Node tempCrossSectionPlane;

    public static int currentIndex = 0;
    public static WidgetSlider scaleSlider;

    EngineWindowViewport timeLine = null;

    private thisTreeGui thisTree;
    

    bool delsetupchush = true;

    void Init()
    {
        thisTree = FindComponentInWorld<thisTreeGui>();
        mainWindow = WindowManager.MainWindow;
        //WidgetHBox viewsFunctions = FuncController.funcViews;
        WidgetHBox statusBarContainer = thisTree.tabMenu2;
        Gui gui = statusBarContainer.Gui;

        labelCrossSection = new WidgetLabel(gui, " Разрез по плоскости") { Width = 130 };
        addCrossButton = new WidgetButton(gui, "Добавить разрез") { Width = 130 };
        delCrossButton = new WidgetButton(gui, "Удалить разрез") { Width = 130 };
        addCrossButton.Height = delCrossButton.Height = 30;

        WidgetVBox functionsCrossSectionVBox = new(gui);
        functionsCrossSectionVBox.SetPosition(180, 8);
        functionsCrossSectionVBox.AddChild(labelCrossSection, Gui.ALIGN_LEFT);
        functionsCrossSectionVBox.AddChild(addCrossButton, Gui.ALIGN_LEFT);
        functionsCrossSectionVBox.AddChild(delCrossButton, Gui.ALIGN_LEFT);
        functionsCrossSectionVBox.SetSpace(0, 5);
        statusBarContainer.AddChild(functionsCrossSectionVBox, Gui.ALIGN_OVERLAP);

        showCrossCheckBox = new WidgetCheckBox(gui) { Checked = true, Text = "Включить разрез" };
        showPlaneCheckBox = new WidgetCheckBox(gui) { Checked = true, Text = "Показать плоскость" };
        fillCheckBox = new WidgetCheckBox(gui) { Checked = true, Text = "Место пересечения" };

        crossSectionType = new(gui);
        crossSectionType.SetPosition(322, 12);
        crossSectionType.AddItem("Разрез");
        statusBarContainer.AddChild(crossSectionType, Gui.ALIGN_OVERLAP);

        functionsCrossVBox = new(gui);
        functionsCrossVBox.SetPosition(320, 28);
        functionsCrossVBox.AddChild(showCrossCheckBox, Gui.ALIGN_LEFT);
        functionsCrossVBox.AddChild(showPlaneCheckBox, Gui.ALIGN_LEFT);
        functionsCrossVBox.AddChild(fillCheckBox, Gui.ALIGN_LEFT);
        functionsCrossVBox.SetSpace(0, 6);
        functionsCrossVBox.Enabled = true;
        statusBarContainer.AddChild(functionsCrossVBox, Gui.ALIGN_OVERLAP);

        crossSections = [];
        countCross = 0;

        addCrossButton.EventClicked.Connect(StartCreatingCrossSection);

        delCrossButton.EventClicked.Connect(DeleteCrossSection);

        crossSectionType.EventChanged.Connect(() =>
        {
            indexCross = crossSectionType.CurrentItem;
            if (indexCross >= 0 && indexCross < crossSections.Count)
            {
                showPlaneCheckBox.Checked = crossSections[indexCross].cross_section_plane.Enabled;
                showCrossCheckBox.Checked = crossSections[indexCross].cross;
                fillCheckBox.Checked = crossSections[indexCross].colorSection;
            }
        });

        WidgetHBox mainFunctions = FuncController.funcMain;

        WidgetButton loadModelButton = new(mainFunctions.Gui, "Загрузить модель");
        loadModelButton.Width = 150;
        loadModelButton.Height = 30;
        loadModelButton.SetPosition(180, 15);
        mainFunctions.AddChild(loadModelButton, Gui.ALIGN_OVERLAP | Gui.ALIGN_TOP);

        WidgetLabel labelScale = new(mainFunctions.Gui, "Масштаб модели:");
        labelScale.SetPosition(200, 60);
        mainFunctions.AddChild(labelScale, Gui.ALIGN_OVERLAP | Gui.ALIGN_TOP);

        scaleSlider = new(mainFunctions.Gui, 1, 30000) { Width = 150 };
        scaleSlider.SetPosition(180, 85);
        mainFunctions.AddChild(scaleSlider, Gui.ALIGN_OVERLAP | Gui.ALIGN_TOP);

        loadModelButton.EventClicked.Connect(() =>
        {
            // importModel.LoadModel();
            // GeniusFunciton();
            scaleSlider.Value = 1;
        });
        scaleSlider.EventChanged.Connect(() =>
        {
            var myNode = test.myNode;
            if (myNode != null)
                myNode.Scale = new vec3(scaleSlider.Value / 1000.0f);
        });

        // CreatePlayer createPlayer = new CreatePlayer();
        // createPlayer.Player();

        Game.Player.AddChild(CUBE);
        CUBE.WorldTransform = Game.Player.WorldTransform + MathLib.Translate(new vec3(0.3, 0.5, 0.2));
        CUBE.Scale = new vec3(1.3);
        CUBE.Enabled = true;

        delsetupchush = false;
    }

    // private void GeniusFunciton()
    // {
    //     if (delsetupchush)
    //     {
    //         // CreatePlayer createPlayer = new CreatePlayer();
    //         // createPlayer.Player();

    //         Game.Player.AddChild(CUBE);
    //         CUBE.WorldTransform = Game.Player.WorldTransform + MathLib.Translate(new vec3(0.3, 0.5, 0.2));
    //         CUBE.Scale = new vec3(1.43);
    //         CUBE.Enabled = true;

    //         delsetupchush = false;
    //     }
    // }

    private void StartCreatingCrossSection()
    {
        if (isCreatingCrossSection) return;

        isCreatingCrossSection = true;
        stopCreatingCrossSection = false;
        tempCrossSectionPlane = World.GetNodeByName("cross_section_plane");
        tempCrossSectionPlane.Enabled = true;

        if (crossSectionType.GetItemText(0) == "Разрез")
        {
            crossSectionType.RemoveItem(0);
            // functionsCrossVBox.Enabled = true;
        }
    }

    private void FinalizeCrossSection()
    {
        stopCreatingCrossSection = true;

        crossSections.Add(new CrossSection());
        indexCross = crossSections.Count - 1;
        countCross = crossSections.Count;

        tempCrossSectionPlane.WorldPosition = new vec3(0, 0, 0);
        tempCrossSectionPlane.Enabled = false;

        crossSectionType.AddItem("Разрез " + countCross);
        crossSectionType.CurrentItem = indexCross;

        showPlaneCheckBox.EventChanged.Connect(() =>
            crossSections[indexCross].cross_section_plane.Enabled = showPlaneCheckBox.Checked);
        showCrossCheckBox.EventChanged.Connect(() =>
        {
            crossSections[indexCross].cross = showCrossCheckBox.Checked;
            var myNode = test.myNode;
            if (myNode != null)
                CrossSection.SetParametersToAllNodes(myNode, crossSections);
        });
        fillCheckBox.EventChanged.Connect(() =>
        {
            LocalSectionUi.fillCheckBox.Checked = fillCheckBox.Checked;
            crossSections[indexCross].OnFillChanged(fillCheckBox.Checked);
        });

        showCrossCheckBox.Checked = crossSections[indexCross].cross;
        showPlaneCheckBox.Checked = crossSections[indexCross].cross_section_plane.Enabled;
        fillCheckBox.Checked = crossSections[indexCross].colorSection;

        isCreatingCrossSection = false;
    }

    private void DeleteCrossSection()
    {
        if (crossSections.Count == 0) return;

        crossSections[indexCross].DeletePlane();
        crossSections.RemoveAt(indexCross);

        crossSectionType.RemoveItem(indexCross);

        countCross--;

        if (countCross <= 0)
        {
            crossSectionType.AddItem("Разрез");
            // functionsCrossVBox.Enabled = false;
        }

        if (indexCross > 0)
        {
            indexCross--;
            crossSectionType.CurrentItem = indexCross;

            showCrossCheckBox.Checked = crossSections[indexCross].cross;
            showPlaneCheckBox.Checked = crossSections[indexCross].cross_section_plane.Enabled;
            fillCheckBox.Checked = crossSections[indexCross].colorSection;
        }

        if (indexCross == 0 && countCross > 0)
        {
            crossSectionType.CurrentItem = 0;
            for (var i = 0; i < crossSections.Count; i++)
            {
                crossSectionType.SetItemText(i, "Разрез " + (i + 1));
            }
        }

        CrossSection.SetParametersToAllNodes(test.myNode, crossSections);
    }

    void Update()
    {
        if (isCreatingCrossSection && !stopCreatingCrossSection)
        {
            // vec3 cursorPos = GetCursorWorldPosition();
            // Node nearestNode = FindNearestNode(cursorPos);

            Node nearestNode = CameraCast.GetNodeUnderCursor();

            if (nearestNode != null && nearestNode.Name == "cross_section_plane")
            {
                nearestNode = CameraCast.GetNodeUnderCursor(CameraCast.GetPointUnderCursor());
            }

            if (nearestNode != null)
            {
                if (nearestNode.Name == "cross_section_plane")
                    nearestNode = CameraCast.GetNodeUnderCursor(CameraCast.GetPointUnderCursor());

                tempCrossSectionPlane.WorldPosition =
                    nearestNode.WorldPosition + tempCrossSectionPlane.GetWorldDirection().Normalized * 0.01;
            }

            if (Input.IsKeyDown(Input.KEY.X))
            {
                tempCrossSectionPlane.SetWorldRotation(new quat(180, 0, 90));
            }
            else if (Input.IsKeyDown(Input.KEY.Y))
            {
                tempCrossSectionPlane.SetWorldRotation(new quat(0, 90, 0));
            }
            else if (Input.IsKeyDown(Input.KEY.Z))
            {
                tempCrossSectionPlane.SetWorldRotation(new quat(90, 0, 0));
            }

            if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT))
            {
                FinalizeCrossSection();
                if (test.myNode != null)
                    CrossSection.SetParametersToAllNodes(test.myNode, crossSections);
            }
        }

        if (test.myNode != null &&
            Manipulators.NameOfObject != null &&
            Manipulators.NameOfObject == "cross_section_plane")
        {
            CrossSection.SetParametersToAllNodes(test.myNode, crossSections);
        }

        if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT) && mainWindow.IsFocused)
        {
            Node node = CameraCast.GetNodeUnderCursor();
            if (node == null) return;
            currentNode = node;
            // if (node.Name != "cross_section_plane" &&
            //     node.Parent.Name != "planes" &&
            //     node.Parent.Name != "main_planes" &&
            //     node.Parent.Name != "Cube" &&
            //     node.Name != "Cube" &&
            //     node.Name != "Main Surface" &&
            //     node.Parent.Name != "Main Surface")
            // {
            //     importModel.SelectNodeColor(node as Object);
            // }

            // if (node.Name == "cross_section_plane")
            // {
            //     importModel.SelectPlaneColor(node as Object);
            // }
        }

        // if (Input.IsMouseButtonDown(Input.MOUSE_BUTTON.LEFT) && !mainWindow.IsFocused)
        // {
        //     int idTab = FuncController.tabBox.CurrentTab;
        //     if (idTab == 0 || idTab == 1 || idTab == 2 || idTab == 3)
        //     {
        //         if (timeLine == null) return;
        //         int height = ConfigManager.config.Video.Height;
        //         windowController.parentGroup.Remove(timeLine);
        //         windowController.parentGroup.SetVerticalTabHeight(0, (int)(height / 4.2));
        //         windowController.parentGroup.UpdateGuiHierarchy();
        //         timeLine.DeleteForce();
        //         timeLine = null;
        //     }
        //     else
        //     {
        //         if (timeLine != null) return;
        //         timeLine = new("timeLine", 100, 100);
        //         int height = ConfigManager.config.Video.Height;
        //         windowController.parentGroup.Add(timeLine);
        //         windowController.parentGroup.SetVerticalTabHeight(0, (int)(height / 3.05));
        //         windowController.parentGroup.SetVerticalTabHeight(2, (int)(height / 3.05));
        //         windowController.parentGroup.UpdateGuiHierarchy();
        //         animationGUI.InitGui();
        //     }
        // }

        if (Input.IsKeyDown(Input.KEY.DELETE) && currentNode != null)
        {
            if (currentNode.Name == "cross_section_plane")
            {
                DeleteCrossSection();
            }
        }

        // if (Input.IsKeyDown(Input.KEY.ESC))
        // {
        //     importModel.HidTreeContextMenu();
        // }

        if (!delsetupchush)
        {
            var rotate = Game.Player.GetRotation();
            vec3 degrees = rotate.Euler * MathLib.RAD2DEG;
            var newRotate = new quat(degrees.x + Storage.degreeCorrectionX, degrees.y, degrees.z + Storage.degreeCorrectionZ);
            CUBE.SetRotation(newRotate);
        }
    }
}