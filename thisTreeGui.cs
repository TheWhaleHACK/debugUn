using System;
using System.Collections;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing;
using Unigine;

[Component(PropertyGuid = "9e102575e5c1425d96663ee2fbd4b5936bb8255c")]
public class thisTreeGui : Component
{
	public MakeOtherObjectsTransparent makeOtherObjectsTransparent;

	public WidgetTreeBox treeUI = new WidgetTreeBox();
	public WidgetTreeBox treeUI2 = new WidgetTreeBox();
	private IntersectionFinder intersectionFinder;
	private PDFViewer pDFViewer;
	private List<Node> _tree;
	private List<Node> _tree2;
	//public WidgetEditText description = new WidgetEditText();
	private WidgetScrollBox widgetScrollBox;
	private WidgetScrollBox widgetScrollBox2;
	public WidgetWindow window;
	private WidgetWindow window2;
	public WidgetWindow descriptionWindow;
	public WidgetButton backspaceBtn, escapeBtn, doubleclickBtn, pdfBtn, resetBtn, pdf3DSwitchBtn;
	public WidgetCanvas canvas, canvas2;
	public WidgetHBox tabMenu = new WidgetHBox();
	public WidgetHBox tabMenu2 = new WidgetHBox();
	private ExcelReader excel;

	// Поля для поиска
	public WidgetEditText searchField;
	private List<string> originalNames = new List<string>();
	private List<string> originalNames2 = new List<string>();
	private List<int> originalIndices = new List<int>();
	private List<int> originalIndices2 = new List<int>();
	private Dictionary<int, int> filteredToOriginalIndices = new Dictionary<int, int>(); // НОВОЕ: Словарь для сопоставления индексов отфильтрованного treeUI с оригинальными индексами

	public bool doubleClicked = false;
	public bool treeClicked = false;
	private Gui gui;

	private WidgetLabel label;

	private TestWindows testWindows;

	void Init()
	{
		// write here code to be called on component initialization
		excel = FindComponentInWorld<ExcelReader>();
		intersectionFinder = FindComponentInWorld<IntersectionFinder>();
		pDFViewer = FindComponentInWorld<PDFViewer>();
		testWindows = FindComponentInWorld<TestWindows>();
		gui = testWindows.headGui;
		canvas = new WidgetCanvas(gui);
		canvas.SetPosition(0, 0);
		canvas.Width = gui.Width;
		canvas.Height = 80;
		canvas.Color = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		canvas2 = new WidgetCanvas(gui);
		canvas2.SetPosition(0, 0);
		canvas2.Width = gui.Width;
		canvas2.Height = 80;
		canvas2.Color = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		tabMenu.AddChild(canvas, Gui.ALIGN_BACKGROUND);
		tabMenu2.AddChild(canvas2, Gui.ALIGN_BACKGROUND);

		WidgetVBox searchDiv = new WidgetVBox(gui);
		label = new WidgetLabel();
		label.Text = "Поиск";
		label.PositionX = 0;
		label.Width = 25;
		label.FontSize = 15;
		searchDiv.AddChild(label, Gui.ALIGN_TOP | Gui.ALIGN_LEFT);

		WidgetHBox buttonLayout = new WidgetHBox();
		buttonLayout.PositionX = label.PositionX;

		// Добавляем строку поиска
		searchField = new WidgetEditText();
		searchField.Height = 20;
		searchField.Width = 120;
		searchField.FontColor = vec4.BLACK;
		searchField.EventChanged.Connect(OnSearchTextChanged); // тоже временный коммент
		buttonLayout.AddChild(searchField);

		// Добавляем кнопку "Очистить"
		WidgetButton clearButton = new WidgetButton();
		clearButton.Text = "<-";
		clearButton.SetToolTip("Очистить");
		clearButton.Width = 30;
		clearButton.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		clearButton.EventClicked.Connect(() =>
		{
			searchField.Text = "";
			FilterTree("");
		});
		buttonLayout.AddChild(clearButton);

		searchDiv.AddChild(buttonLayout, Gui.ALIGN_TOP | Gui.ALIGN_EXPAND);
		tabMenu.AddChild(searchDiv, Gui.ALIGN_LEFT | Gui.ALIGN_TOP);

		WidgetHBox divcontainer = new WidgetHBox(gui);
		divcontainer.Width = gui.Width;
		divcontainer.Height = 50;



		pdfBtn = new WidgetButton(gui);
		pdfBtn.Text = "Добавить PDF";
		pdfBtn.SetPosition(350, 15);
		pdfBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		pdfBtn.EventClicked.Connect(ButtonPdfClick_event_handler);
		divcontainer.AddChild(pdfBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		resetBtn = new WidgetButton(gui);
		resetBtn.Text = "Вернуть все окна по умолчанию";
		resetBtn.SetPosition(pdfBtn.PositionX + 180, pdfBtn.PositionY);
		resetBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		resetBtn.EventClicked.Connect(ButtonResetClick_event_handler);
		divcontainer.AddChild(resetBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		pdf3DSwitchBtn = new WidgetButton(gui);
		pdf3DSwitchBtn.Text = "Скрыть чертеж";
		pdf3DSwitchBtn.SetPosition(resetBtn.PositionX + 250, pdfBtn.PositionY);
		pdf3DSwitchBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		pdf3DSwitchBtn.EventClicked.Connect(ButtonPdf3DSwitchClick_event_handler);
		divcontainer.AddChild(pdf3DSwitchBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		pdf3DSwitchBtn.Hidden = true;

		tabMenu.AddChild(divcontainer, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		WidgetHBox divcontainer2 = new WidgetHBox(gui);
		divcontainer2.Width = gui.Width;
		divcontainer2.Height = 50;

		backspaceBtn = new WidgetButton(gui);
		backspaceBtn.Text = "На уровень выше";
		backspaceBtn.SetPosition(100, 15);
		backspaceBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		backspaceBtn.EventClicked.Connect(backspace_event_handler); // вот тут backspace
		divcontainer2.AddChild(backspaceBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		escapeBtn = new WidgetButton(gui);
		escapeBtn.Text = "Отменить выделение";
		escapeBtn.SetPosition(backspaceBtn.PositionX + 150, backspaceBtn.PositionY);
		escapeBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		escapeBtn.EventClicked.Connect(escape_event_handler);
		divcontainer2.AddChild(escapeBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		doubleclickBtn = new WidgetButton(gui);
		doubleclickBtn.Text = "Просмотр детали";
		doubleclickBtn.SetPosition(escapeBtn.PositionX + 180, backspaceBtn.PositionY);
		doubleclickBtn.ButtonColor = new vec4(80f / 255f, 129f / 255f, 166f / 255f, 1.0f);
		doubleclickBtn.EventClicked.Connect(ButtonDoubleClick_event_handler);
		divcontainer2.AddChild(doubleclickBtn, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		tabMenu2.AddChild(divcontainer2, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);

		testWindows.headerWindow.AddChild(canvas, Gui.ALIGN_LEFT | Gui.ALIGN_BACKGROUND | Gui.ALIGN_EXPAND);
		testWindows.headerWindow.AddChild(tabMenu, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);
		testWindows.statusBar.AddChild(canvas2, Gui.ALIGN_LEFT | Gui.ALIGN_BACKGROUND | Gui.ALIGN_EXPAND);
		testWindows.statusBar.AddChild(tabMenu2, Gui.ALIGN_OVERLAP | Gui.ALIGN_FIXED);


		// Инициализация GUI
		treeUI = new WidgetTreeBox(gui);
		treeUI2 = new WidgetTreeBox(gui);

		widgetScrollBox = new WidgetScrollBox(gui);
		widgetScrollBox2 = new WidgetScrollBox(gui);

		window = new WidgetWindow(gui, "Дерево сцены");

		window2 = new WidgetWindow(gui, "Дерево сцены (стабильное)");
		descriptionWindow = new WidgetWindow(gui, "Описание");

		// Настройка окна дерева
		widgetScrollBox.Width = 200;
		widgetScrollBox2.Width = 200;

		widgetScrollBox.Height = gui.Height - 50;
		widgetScrollBox2.Height = gui.Height - 50;


		treeUI.Editable = true;
		treeUI.FontColor = vec4.BLACK;
		treeUI2.Editable = true;

		treeUI.MultiSelection = true;
		treeUI2.MultiSelection = true;

		treeUI.EventClicked.Connect(clicked_event_handler);
		treeUI.EventEnter.Connect(mouseenter_event_handler);
		treeUI.EventLeave.Connect(mouseleave_event_handler);
		treeUI.EventDoubleClicked.Connect(doubleclicked_event_handler);
		// treeUI2.EventChanged.Connect(changed_event_handler);

		widgetScrollBox.AddChild(treeUI);
		widgetScrollBox2.AddChild(treeUI2);

		window.AddChild(widgetScrollBox, Gui.ALIGN_BOTTOM | Gui.ALIGN_EXPAND);
		window2.AddChild(widgetScrollBox2, Gui.ALIGN_EXPAND);

		window.Sizeable = true;
		window2.Sizeable = true;
		window.Width = testWindows.mainGroup.GetTabWidth(1);
		window.Height = testWindows.treeWindow.Size.y;

		// Позиционирование первого дерева (без изменений)
		testWindows.treeWindow.AddChild(window, Gui.ALIGN_EXPAND);

		// Настройка окна описания
		// description = new WidgetEditText(gui);
		descriptionWindow.Width = testWindows.window.Size.x;
		descriptionWindow.Height = 100;
		descriptionWindow.PositionX = window.PositionX;
		descriptionWindow.PositionY = window.PositionY;
		// description.FontColor = vec4.GREY;
		// description.Text = "Тут должно быть описание";
		// descriptionWindow.AddChild(description);

		//testWindows.window.AddChild(descriptionWindow, Gui.ALIGN_BOTTOM | Gui.ALIGN_OVERLAP);
	}

	void ButtonResetClick_event_handler(Widget widget)
	{
		testWindows.ResetUILayout();
	}

	void ButtonPdf3DSwitchClick_event_handler(Widget widget)
	{

    if (pdf3DSwitchBtn.Text == "Скрыть чертеж")
    {
        // Показываем 3D-окно (mainMenu), скрываем PDF
        testWindows.mainGroup.SetHorizontalTabWidth(0, testWindows.window0_save);
			testWindows.mainGroup.BordersEnabled = false;
        testWindows.mainGroup.SetHorizontalTabWidth(1, testWindows.window1_save);  
        testWindows.mainGroup.SetHorizontalTabWidth(2, testWindows.window2_save-testWindows.window1_save);    
        testWindows.mainGroup.SetHorizontalTabWidth(3, 0);

        pdf3DSwitchBtn.Text = "Показать чертеж";
		
    }
    else if (pdf3DSwitchBtn.Text == "Показать чертеж")
    {
        testWindows.mainGroup.SetHorizontalTabWidth(0, testWindows.window0_save);   
        testWindows.mainGroup.SetHorizontalTabWidth(1, 0);
        testWindows.mainGroup.SetHorizontalTabWidth(2, 0);
        testWindows.mainGroup.SetHorizontalTabWidth(3, testWindows.window1_save+testWindows.window2_save);     
        pdf3DSwitchBtn.Text = "Скрыть чертеж";
    }

    testWindows.mainGroup.UpdateGuiHierarchy();
	}

	void ButtonPdfClick_event_handler(Widget widget)
	{
		pDFViewer.window.Hidden = false;
	}
	void Update()
	{
		if (!treeClicked)
			widgetScrollBox.VScrollEnabled = false;
		else
			widgetScrollBox.VScrollEnabled = true;
	}

	private void OnSearchTextChanged(Widget widget)
	{
		FilterTree(searchField.Text);
	}

    public void UnfoldHierarhy(int index)
    {
        if (index >= 0 && treeUI.GetItemParent(index) >= 0)
        {
            treeUI.SetItemFolded(treeUI.GetItemParent(index), 0);
            treeUI.ShowItem(index);
            UnfoldHierarhy(treeUI.GetItemParent(index));
            Log.Message($"Unfolded treeUI hierarchy for index: {index}\n");
        }

        if (index >= 0 && treeUI2.GetItemParent(index) >= 0)
        {
            treeUI2.SetItemFolded(treeUI2.GetItemParent(index), 0);
            treeUI2.ShowItem(index);
            UnfoldHierarhy(treeUI2.GetItemParent(index));
            Log.Message($"Unfolded treeUI2 hierarchy for index: {index}\n");
        }
    }

    public void CreateTree(List<Node> tree)
	{
		_tree = new List<Node>(tree);
		_tree2 = new List<Node>(tree);

		Log.Message("TreeGui.CreateTree: Получено {0} нод\n", _tree.Count);
		foreach (Node node in _tree)
		{
			Log.Message("Нода в дереве: {0}\n", node.Name);
		}

		treeUI.Clear();
		treeUI2.Clear();
		originalNames.Clear();
		originalNames2.Clear();
		originalIndices.Clear();
		originalIndices2.Clear();

		// Добавляем элементы в treeUI
		foreach (Node node in _tree)
		{
			int itemIndex = treeUI.AddItem(node.Name);
			treeUI.SetItemData(itemIndex, node.Name);
			originalNames.Add(node.Name);
			originalIndices.Add(itemIndex);
			Log.Message($"Added to treeUI: {node.Name}, index: {itemIndex}\n");
		}

		// Добавляем элементы в treeUI2
		foreach (Node node in _tree2)
		{
			int itemIndex = treeUI2.AddItem(node.Name);
			treeUI2.SetItemData(itemIndex, node.Name);
			originalNames2.Add(node.Name);
			originalIndices2.Add(itemIndex);
			Log.Message($"Added to treeUI2: {node.Name}, index: {itemIndex}\n");
		}

		// Настраиваем иерархию для treeUI
		foreach (Node node in _tree)
		{
			if (_tree.Contains(node.Parent))
			{
				int parentIndex = _tree.FindIndex(x => x == node.Parent);
				int nodeIndex = _tree.FindIndex(x => x == node);
				treeUI.AddItemChild(parentIndex, nodeIndex);
				treeUI.SetItemFolded(parentIndex, 1);
				Log.Message($"treeUI hierarchy: child {node.Name} to parent {node.Parent.Name}, parentIndex: {parentIndex}, nodeIndex: {nodeIndex}\n");
			}
		}

		// Настраиваем иерархию для treeUI2
		foreach (Node node in _tree2)
		{
			if (_tree2.Contains(node.Parent))
			{
				int parentIndex = _tree2.FindIndex(x => x == node.Parent);
				int nodeIndex = _tree2.FindIndex(x => x == node);
				treeUI2.AddItemChild(parentIndex, nodeIndex);
				treeUI2.SetItemFolded(parentIndex, 1);
				Log.Message($"treeUI2 hierarchy: child {node.Name} to parent {node.Parent.Name}, parentIndex: {parentIndex}, nodeIndex: {nodeIndex}\n");
			}
		}
	}

	private void FilterTree(string query)
	{
		treeUI.Clear();
		filteredToOriginalIndices.Clear(); // Очищаем словарь перед новой фильтрацией
		Log.Message($"FilterTree: Query = '{query}'\n");

		if (string.IsNullOrWhiteSpace(query))
		{
			// Восстанавливаем оригинальное дерево для treeUI
			for (int i = 0; i < originalNames.Count; i++)
			{
				int index = treeUI.AddItem(originalNames[i]);
				treeUI.SetItemData(index, originalNames[i]);
				filteredToOriginalIndices.Add(index, i); // Сохраняем соответствие: новый индекс в treeUI -> оригинальный индекс
				Log.Message($"Restored to treeUI: {originalNames[i]}, index: {index}\n");
			}

			// Восстанавливаем иерархию для treeUI
			for (int i = 0; i < _tree.Count; i++)
			{
				Node node = _tree[i];
				if (_tree.Contains(node.Parent))
				{
					int parentIndex = _tree.FindIndex(x => x == node.Parent);
					int nodeIndex = i;
					treeUI.AddItemChild(parentIndex, nodeIndex);
					treeUI.SetItemFolded(parentIndex, 1);
					Log.Message($"Restored treeUI hierarchy: child {node.Name} to parent {node.Parent.Name}, parentIndex: {parentIndex}, nodeIndex: {nodeIndex}\n");
				}
			}
		}
		else
		{
			query = query.ToLower();
			Dictionary<int, int> visibleIndices = new Dictionary<int, int>();

			// Добавляем подходящие элементы в treeUI
			for (int i = 0; i < originalNames.Count; i++)
			{
				if (originalNames[i].ToLower().Contains(query))
				{
					int newItemIndex = treeUI.AddItem(originalNames[i]);
					// мини база данных. Ключ - новый индекс в первом дереве. Значение - индекс во втором дереве(которое не изменяется)
					treeUI.SetItemData(newItemIndex, originalNames[i]);
					filteredToOriginalIndices.Add(newItemIndex, i); // Сохраняем соответствие: новый индекс в treeUI -> оригинальный индекс
					visibleIndices.Add(i, newItemIndex);
					Log.Message($"Filtered to treeUI: {originalNames[i]}, newItemIndex: {newItemIndex}, originalIndex: {i}\n");
				}
			}

			// Восстанавливаем иерархию для видимых узлов в treeUI
			foreach (int i in visibleIndices.Keys)
			{
				Node node = _tree[i];
				if (_tree.Contains(node.Parent))
				{
					int parentOriginalIndex = _tree.FindIndex(x => x == node.Parent);
					if (visibleIndices.ContainsKey(parentOriginalIndex))
					{
						int parentNewItemIndex = visibleIndices[parentOriginalIndex];
						int childNewItemIndex = visibleIndices[i];
						treeUI.AddItemChild(parentNewItemIndex, childNewItemIndex);
						treeUI.SetItemFolded(parentNewItemIndex, 0);
						Log.Message($"Filtered treeUI hierarchy: child {node.Name} to parent {node.Parent.Name}, parentNewItemIndex: {parentNewItemIndex}, childNewItemIndex: {childNewItemIndex}\n");
					}
				}
			}
		}

		// treeUI2 остается нетронутым
		Log.Message("treeUI2 remains unchanged\n");
	}

	// void changed_event_handler(Widget widget)
	// {
	// 	WidgetTreeBox tree = widget as WidgetTreeBox;
	// }

	void backspace_event_handler(Widget button)
	{
		makeOtherObjectsTransparent.BackspaceMode();
	}

	void escape_event_handler(Widget button)
	{
		makeOtherObjectsTransparent.EscapeMode();
	}

	void mouseleave_event_handler(Widget widget)
	{
		treeClicked = false;
	}

	void mouseenter_event_handler(Widget widget)
	{
		treeClicked = true;
	}

	void doubleclicked_event_handler(Widget widget)
	{
		doubleClicked = true;
	}

	void clicked_event_handler(Widget widget)
	{
		//doubleClicked = true;//временно
		if (!doubleClicked)
			intersectionFinder.doubleClicked = false;
		try
		{

			makeOtherObjectsTransparent.UntransparentAll();
			int index = GetItemUnderCursor(widget);
			if (Input.IsKeyDown(Input.KEY.BACKSPACE))
				if (index < 0)
				{
					Log.Message("Invalid index under cursor\n");
					return;
				}

			Node node = null;
			int originalIndex = -1;

			Log.MessageLine("indexnew " + index);
			Log.MessageLine("originalindex " + originalIndex);
			WidgetTreeBox tree = widget as WidgetTreeBox;
			if (tree == null)
			{
				Log.Error("Widget is not a WidgetTreeBox\n");
				return;
			}
			bool isTreeUI = (tree == treeUI);
			bool emptySearch = string.IsNullOrWhiteSpace(searchField.Text);
			if (isTreeUI && !emptySearch)
			{
				// Обработка клика в treeUI с активным поиском
				if (!filteredToOriginalIndices.ContainsKey(index))
				{
					Log.Error($"Index {index} not found in filteredToOriginalIndices\n");
					return;
				}
				originalIndex = filteredToOriginalIndices[index]; // Получаем оригинальный индекс из словаря (ключ - index, значение - filteredToOriginalIndices[index])
				if (originalIndex < 0 || originalIndex >= _tree.Count)
				{
					Log.Error($"Original index out of bounds: {originalIndex}, _tree.Count: {_tree.Count}\n");
					return;
				}
				node = _tree2[originalIndex];
				if (node == null)
				{
					Log.Error($"Node at original index {originalIndex} is null\n");
					return;
				}
				Log.Message($"Selected from filtered treeUI: {node.Name}, filtered index: {index}, original index: {originalIndex}\n");
			}
			else
			{
				// Обработка клика без поиска или в treeUI2
				List<Node> targetTree = isTreeUI ? _tree : _tree2;
				if (index >= targetTree.Count)
				{
					Log.Error($"Index out of bounds in {(isTreeUI ? "treeUI" : "treeUI2")}: {index}, tree count: {targetTree.Count}\n");
					return;
				}
				node = targetTree[index];
				if (node == null)
				{
					Log.Error($"Node at index {index} in {(isTreeUI ? "treeUI" : "treeUI2")} is null\n");
					return;
				}
				originalIndex = index;
				//Log.Message($"Selected from {(isTreeUI ? "treeUI" : "treeUI2")} (no search): {node.Name}, index: {index}\n");
			}

			// Синхронизируем выбор в обоих деревьях
			SelectItemFromModel(originalIndex);
			if (node.NumChildren > 0)
			{
				Log.Message($"Node has {node.NumChildren} children\n");
				List<Unigine.Object> childObjects = new List<Unigine.Object>();
				CollectChildObjects(node, childObjects);// вот тут нода, которую надо добавить в HierarchiStack
				Log.Message($"Found {childObjects.Count} Unigine.Object children\n");

				if (childObjects.Count > 0 && makeOtherObjectsTransparent != null)
				{
					makeOtherObjectsTransparent.IsolateParts(childObjects);
					Log.Message($"Isolated {childObjects.Count} child objects for node: {node.Name}\n");

					if (intersectionFinder != null && intersectionFinder.player != null)
					{
						intersectionFinder.player.Target = intersectionFinder.targetNode;
						intersectionFinder.LastSelected = childObjects[0];
						//excel.ShowDescriptionFor(childObjects[0].Name); // надо чтобы не в отдельном окне выводилось, а в этом менялся /backgroundcolor, например на желтый
						
						//description.Text = childObjects[0].Name;
						Log.Message($"IntersectionFinder updated with child object: {childObjects[0].Name}\n");
					}
					else
					{
						Log.Message("IntersectionFinder or player is null\n");
					}
				}
				else
				{
					Log.Message("No Unigine.Object children found\n");
				}
			}
			else
			{
				Log.Message("Node has no children\n");
				if (node is Unigine.Object obj)//некоторые не кликаются, не изолируются потому что они тупо могут быть не объектами Object
				{
					if (makeOtherObjectsTransparent != null)
					{
						makeOtherObjectsTransparent.IsolatePart(obj);
						Log.MessageLine(" BB " + (obj.BoundBox.maximum - obj.BoundBox.minimum)); // выводит размеры детальки(в x,y,z)
						Log.Message($"Isolated node: {obj.Name}\n");

						if (intersectionFinder != null && intersectionFinder.player != null)
						{
							intersectionFinder.player.Target = intersectionFinder.targetNode;
							intersectionFinder.LastSelected = obj;
							//description.Text = obj.Name;
							//excel.ShowDescriptionFor(obj.Name); // надо чтобы не в отдельном окне выводилось, а в этом менялся /backgroundcolor, например на желтый ТУТ ВАЖНЕЕ
							Log.Message("IntersectionFinder updated for node\n");
						}
						else
						{
							Log.Message("IntersectionFinder or player is null\n");
						}
					}
					else
					{
						Log.Message("makeOtherObjectsTransparent is null\n");
					}
				}
				else
				{
					Log.Message($"Node is not a Unigine.Object, type: {node.Type}\n");
				}
			}

		}
		catch (Exception e)
		{
			Log.Error($"Error in clicked_event_handler: {e.Message}\n{e.StackTrace}\n");
		}
	}

	public void SelectItemFromModel(int originalIndex)
	{
		try
		{
			Log.Message($"SelectItemFromModel called with originalIndex: {originalIndex}\n");

			if (originalIndex < 0 || originalIndex >= _tree.Count)
			{
				Log.Error($"Invalid originalIndex: {originalIndex}, _tree.Count: {_tree.Count}\n");
				return;
			}

			string nodeName = _tree[originalIndex].Name;
			if (string.IsNullOrEmpty(nodeName))
			{
				Log.Error($"Node name at index {originalIndex} is null or empty\n");
				return;
			}

			// === treeUI ===
			if (!string.IsNullOrWhiteSpace(searchField.Text))
			{
				// Поиск активен — нужно найти, есть ли этот originalIndex в отфильтрованном дереве
				bool found = false;
				foreach (var pair in filteredToOriginalIndices)
				{
					if (pair.Value == originalIndex)
					{
						// Снимаем выделение со всех
						List<int> allItems = new List<int>();
						treeUI.GetItems(allItems);
						foreach (int i in allItems)
						{
							treeUI.SetItemSelected(i, 0);
						}
						// Выбираем нужный
						treeUI.SetItemSelected(pair.Key, 1);
						treeUI.ShowItem(pair.Key); // Прокручиваем к элементу
						Log.Message($"Selected filtered item in treeUI: {nodeName}, filtered index: {pair.Key}\n");
						found = true;
						break;
					}
				}
				if (!found)
				{
					Log.Message($"Item {nodeName} not visible in filtered treeUI (filtered out)\n");
				}
			}
			else
			{
				// Поиск не активен — можно использовать originalIndex как индекс
				List<int> allItems = new List<int>();
				treeUI.GetItems(allItems);
				if (originalIndex < allItems.Count)
				{
					foreach (int i in allItems)
					{
						treeUI.SetItemSelected(i, 0);
					}
					treeUI.SetItemSelected(originalIndex, 1);
					treeUI.ShowItem(originalIndex);
					Log.Message($"Selected item in treeUI (no filter): {nodeName}, index: {originalIndex}\n");
				}
				else
				{
					Log.Error($"originalIndex {originalIndex} out of bounds in treeUI: {allItems.Count}\n");
				}
			}

			// === treeUI2 === (никогда не фильтруется)
			List<int> items2 = new List<int>();
			treeUI2.GetItems(items2);
			foreach (int i in items2)
			{
				treeUI2.SetItemSelected(i, 0);
			}
			if (originalIndex < items2.Count)
			{
				treeUI2.SetItemSelected(originalIndex, 1);
				treeUI2.ShowItem(originalIndex);
				Log.Message($"Selected item in treeUI2: {nodeName}, index: {originalIndex}\n");
			}
			else
			{
				Log.Error($"originalIndex {originalIndex} out of bounds in treeUI2: {items2.Count}\n");
			}
		}
		catch (Exception e)
		{
			Log.Error($"Error in SelectItemFromModel: {e.Message}\n{e.StackTrace}\n");
		}
	}

	private int GetItemUnderCursor(Widget widget)
	{
		try
		{
			WidgetTreeBox tree = widget as WidgetTreeBox;
			if (tree == null)
			{
				Log.Error("Widget is not a WidgetTreeBox\n");
				return -1;
			}

			int itemIndex = tree.ItemUnderCursor;
			if (itemIndex < 0)
			{
				Log.Message($"No item under cursor in {(tree == treeUI ? "treeUI" : "treeUI2")}\n");
				return -1;
			}

			List<int> items = new List<int>();
			tree.GetItems(items);

			if (itemIndex >= 0 && itemIndex < items.Count)
			{
				Log.Message($"Item under cursor in {(tree == treeUI ? "treeUI" : "treeUI2")}: {tree.GetItemData(itemIndex)}, index: {itemIndex}\n");
				return itemIndex;
			}

			Log.Message($"Item index out of bounds in {(tree == treeUI ? "treeUI" : "treeUI2")}: {itemIndex}\n");
			// Убедимся, что индекс соответствует существующему элементу
			return itemIndex < tree.NumItems ? itemIndex : -1;
		}
		catch (Exception e)
		{
			Log.Error($"Error in GetItemUnderCursor: {e.Message}\n{e.StackTrace}\n");
			return -1;
		}
	}

	private void CollectChildObjects(Node node, List<Unigine.Object> childObjects)
	{
		for (int i = 0; i < node.NumChildren; i++)
		{
			Node child = node.GetChild(i);
			if (child is Unigine.Object obj)
			{
				childObjects.Add(obj);
				//Log.Message($"Collected child object: {obj.Name}, Type: {obj.Type}\n");
			}
			CollectChildObjects(child, childObjects);
		}
	}

	void ButtonDoubleClick_event_handler(Widget widget)
	{
		doubleClicked = true;
		int selectedItemIndex = treeUI.GetSelectedItem(0);
		if (selectedItemIndex >= 0 && selectedItemIndex < _tree.Count)
		{
			Node selectedNode = _tree[selectedItemIndex];
			HandleDoubleClick(selectedNode);
		}
		else
		{
			Log.Error("No item selected or invalid index for button double click action.");
		}
	}

    private void HandleDoubleClick(Node node)
    {
        if (node == null)
        {
            Log.Error("Node is null in HandleDoubleClick");
            return;
        }

        // Здесь повторяем то, что делается при даблклике внутри clicked_event_handler

        if (node.NumChildren > 0)
        {
            List<Unigine.Object> childObjects = new List<Unigine.Object>();
            CollectChildObjects(node, childObjects);
            if (childObjects.Count > 0 && makeOtherObjectsTransparent != null)
            {
                makeOtherObjectsTransparent.IsolateParts(childObjects);
                if (intersectionFinder != null && intersectionFinder.player != null)
                {
                    //intersectionFinder.player.Target = intersectionFinder.targetNode;
                    intersectionFinder.LastSelected = childObjects[0];
                }
            }
        }
        else
        {
            if (node is Unigine.Object obj)
            {
                if (makeOtherObjectsTransparent != null)
                {
                    makeOtherObjectsTransparent.IsolatePart(obj);
                    if (intersectionFinder != null && intersectionFinder.player != null)
                    {
                        //intersectionFinder.player.Target = intersectionFinder.targetNode;
                        intersectionFinder.LastSelected = obj;
                    }
                }
            }
        }
    }		
}