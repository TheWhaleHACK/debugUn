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

    private EngineWindowViewport mainWindow;
    private Node currentNode;

    public static List<CrossSection> crossSections;
    public static int countCross;
    public static int indexCross;

    public static WidgetComboBox crossSectionType;
    public static WidgetCheckBox fillCheckBox;
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
    private bool guiInitialized = false;

    void Init()
    {
        mainWindow = WindowManager.MainWindow;
        crossSections = new List<CrossSection>();
        countCross = 0;

        // Остальная логика инициализации (не GUI)
        Game.Player.AddChild(CUBE);
        CUBE.WorldTransform = Game.Player.WorldTransform + MathLib.Translate(new vec3(0.3f, 0.5f, 0.2f));
        CUBE.Scale = new vec3(1.3f);
        CUBE.Enabled = true;
    }

    void Update()
    {
        // Отложенная инициализация GUI — ждём, пока thisTreeGui загрузится
        if (!guiInitialized && thisTree == null)
        {
            thisTree = FindComponentInWorld<thisTreeGui>();
            if (thisTree != null && thisTree.tabMenu2 != null)
            {
                InitializeCrossSectionGui();
                guiInitialized = true;
            }
        }

        // Логика создания/удаления разрезов
        if (isCreatingCrossSection && !stopCreatingCrossSection)
        {
            Node nearestNode = CameraCast.GetNodeUnderCursor();

            if (nearestNode != null)
            {
                if (nearestNode.Name == "cross_section_plane")
                    nearestNode = CameraCast.GetNodeUnderCursor(CameraCast.GetPointUnderCursor());

                tempCrossSectionPlane.WorldPosition =
                    nearestNode.WorldPosition + tempCrossSectionPlane.GetWorldDirection().Normalized * 0.01f;
            }

            if (Input.IsKeyDown(Input.KEY.X))
                tempCrossSectionPlane.SetWorldRotation(new quat(180, 0, 90));
            else if (Input.IsKeyDown(Input.KEY.Y))
                tempCrossSectionPlane.SetWorldRotation(new quat(0, 90, 0));
            else if (Input.IsKeyDown(Input.KEY.Z))
                tempCrossSectionPlane.SetWorldRotation(new quat(90, 0, 0));

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
            if (node != null)
                currentNode = node;
        }

        if (Input.IsKeyDown(Input.KEY.DELETE) && currentNode != null && currentNode.Name == "cross_section_plane")
        {
            DeleteCrossSection();
        }

        // Обновление поворота куба
        if (!false) // delsetupchush всегда false → упрощаем
        {
            var rotate = Game.Player.GetRotation();
            vec3 degrees = rotate.Euler * MathLib.RAD2DEG;
            var newRotate = new quat(degrees.x + Storage.degreeCorrectionX, degrees.y, degrees.z + Storage.degreeCorrectionZ);
            CUBE.SetRotation(newRotate);
        }
    }

    private void InitializeCrossSectionGui()
    {
        WidgetVBox statusBarContainer = thisTree.tabMenu2;
        Gui gui = statusBarContainer.Gui;

        // --- Группа элементов разреза ---
        WidgetHBox crossSectionGroup = new(gui);
        crossSectionGroup.SetSpace(8, 0); // Горизонтальный отступ между элементами

        // Метка и кнопки
        labelCrossSection = new(gui, "Разрез по плоскости");
        addCrossButton = new(gui, "Добавить разрез") { Width = 130, Height = 30 };
        delCrossButton = new(gui, "Удалить разрез") { Width = 130, Height = 30 };

        crossSectionGroup.AddChild(labelCrossSection, Gui.ALIGN_LEFT);
        crossSectionGroup.AddChild(addCrossButton, Gui.ALIGN_LEFT);
        crossSectionGroup.AddChild(delCrossButton, Gui.ALIGN_LEFT);

        // Чекбоксы
        showCrossCheckBox = new(gui) { Text = "Включить разрез", Checked = true };
        showPlaneCheckBox = new(gui) { Text = "Показать плоскость", Checked = true };
        fillCheckBox = new(gui) { Text = "Место пересечения", Checked = true };

        crossSectionGroup.AddChild(showCrossCheckBox, Gui.ALIGN_LEFT);
        crossSectionGroup.AddChild(showPlaneCheckBox, Gui.ALIGN_LEFT);
        crossSectionGroup.AddChild(fillCheckBox, Gui.ALIGN_LEFT);

        // Комбобокс
        crossSectionType = new(gui);
        crossSectionType.AddItem("Разрез");
        crossSectionGroup.AddChild(crossSectionType, Gui.ALIGN_LEFT);

        //вот нижние 2 должны быть не по вертикали друг за другом добавлены, а по горизонтали( не в строчку а в столбец)
        statusBarContainer.AddChild(crossSectionGroup);
        statusBarContainer.AddChild(thisTree.divcontainer2);
        statusBarContainer.SetSpace(0, 5);

        // Подписки
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

        // --- Элементы в funcMain (остаются как есть) ---
        WidgetHBox mainFunctions = FuncController.funcMain;

        WidgetButton loadModelButton = new(mainFunctions.Gui, "Загрузить модель") { Width = 150, Height = 30 };
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
            scaleSlider.Value = 1;
        });

        scaleSlider.EventChanged.Connect(() =>
        {
            var myNode = test.myNode;
            if (myNode != null)
                myNode.Scale = new vec3(scaleSlider.Value / 1000.0f);
        });
    }

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
        }

        if (indexCross > 0)
        {
            indexCross--;
            crossSectionType.CurrentItem = indexCross;
            showCrossCheckBox.Checked = crossSections[indexCross].cross;
            showPlaneCheckBox.Checked = crossSections[indexCross].cross_section_plane.Enabled;
            fillCheckBox.Checked = crossSections[indexCross].colorSection;
        }
        else if (countCross > 0)
        {
            crossSectionType.CurrentItem = 0;
            for (int i = 0; i < crossSections.Count; i++)
            {
                crossSectionType.SetItemText(i, "Разрез " + (i + 1));
            }
        }

        CrossSection.SetParametersToAllNodes(test.myNode, crossSections);
    }
}
