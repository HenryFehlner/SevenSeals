extends CanvasLayer

#region UI Reference
@onready var coloringCanvas : Control = $ColoringCanvas
@onready var lineArtLayer:CanvasLayer = $LineArt
@onready var drawingContainer:Control = $LineArt/ColoringContainer

#region Drawing State
var currentPage:int = 0
var currentColor: Color = Color.RED
var brushSize: int = 10
var isDrawing: bool = false
var drawingMode: String = "bucket"
var undoStack: Array[Image] = []
var redoStack: Array[Image] = []
var maxUndoSteps: int = 200

#region Page Data
#can only put pictures in this array
var coloringLayers: Array[Image] = []
#will hold all the line art
var pages: Array[Image] = [
	
	preload("res://coloring-images/pikachu2.png").get_image(),
	preload("res://coloring-images/mareanie.png").get_image(),
	preload("res://coloring-images/slyveon.png").get_image()
]
#size of the canvas, will be able to change it to mobile easily
var canvasSize: Vector2 = Vector2(400, 490)

#region Drawing Properties
var lineMask: Image
var lineWidth: float = 4.0
var lineColor: Color = Color.BLACK
var floodFillActive:bool = false

#region signals
signal layersLoaded

#region functions
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	createPageDef()
	drawPage(0)

#creates a page to attach to the coloring layer
func createPageDef()-> void:
	coloringLayers.clear()
	#creates an specific page for each line art coloring oage.
	for i:int in range(pages.size()):
		#This will create each new page, size, and that it contains red, blue and yellow 
		var img: Image = Image.create(int(canvasSize.x),int(canvasSize.y),false,Image.FORMAT_RGBA8)
		img.fill(Color.TRANSPARENT)
		coloringLayers.append(img)
		

#Creates the coloring layer so you can draw on top of the line art layer 
func setColoringLayer() -> void:
	for c:TextureRect in coloringCanvas.get_children():
		c.queue_free()
		
	var textRect:TextureRect = TextureRect.new()
	textRect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	textRect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	coloringCanvas.add_child(textRect)
	#waits one frame to let the image load in
	await get_tree().process_frame
	textRect.name = "ColoringLayer"
	updateColorDisplay()
	#makes sure it's being rendered ontop 
	lineArtLayer.layer = 1
		
#Converts the image into a texture
func updateColorDisplay() -> void:
	var display:TextureRect = coloringCanvas.get_node("ColoringLayer")
	if display:
		var text:ImageTexture = ImageTexture.new()
		text.set_image(coloringLayers[currentPage])
		display.texture = text
		
	
#Displays the actual texture	
func drawPage(pageNumber:int) -> void:
	if pageNumber < 0 or pageNumber >= pages.size():
		return
	currentPage = pageNumber
	clearDrawingContainer()
	var img:Image = pages[currentPage]
	img.convert(Image.FORMAT_RGBA8)
	img.resize(int(canvasSize.x), int(canvasSize.y))
	lineMask = img.duplicate()
	
	var text:ImageTexture = ImageTexture.new()
	text = ImageTexture.create_from_image(img)
	var textRect:TextureRect = TextureRect.new()
	textRect.texture = text
	textRect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	textRect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	#textRect.texture_filter = CanvasItem.TEXTURE_FILTER_NEAREST
	textRect.z_index = 0
	await get_tree().process_frame
	textRect.name = "LineArtImage"
	drawingContainer.add_child(textRect)
	
	setColoringLayer()
	
#Clears the drawing container
func clearDrawingContainer() -> void: 
	for child:TextureRect in drawingContainer.get_children():
		drawingContainer.remove_child(child)
		child.queue_free()
	
#currently checks for mouse movement
func _input(event: InputEvent) -> void:
	if floodFillActive:
		return
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if isMouseOverUi(event.position):
				return
				
			#for coloring
			if event.pressed:
				startColoring(event.position)
				print("coloring work")
			else:
				stopColoring()
	
	elif event is InputEventMouseMotion and isDrawing:
		if isMouseOverUi(event.position):
			return
			
		continueColoring(event.position)
	
#Checks to see if the mouse is over the UI
#Can be changed to finger
func isMouseOverUi(mousePos: Vector2) -> bool:
	if $ToolBar and $ToolBar.get_global_rect().has_point(mousePos):
		return true
	if $ColorToolbar and $ColorToolbar.get_global_rect().has_point(mousePos):
		return true
		
	return false
	
#gets the coloring position from the image
func getColoringPosition(screenPos: Vector2) -> Vector2:
	var textRect = drawingContainer.get_node("LineArtImage")
	if textRect:
		var imgSize: Vector2 = textRect.texture.get_size()
		var rectSize: Vector2 = textRect.size
		var cScale = min(rectSize.x/imgSize.x, rectSize.y/imgSize.y )
		var coffset = (rectSize - imgSize * cScale)/2
		var local = (screenPos - textRect.global_position - coffset)/cScale
		return Vector2(
			clamp(local.x, 0, canvasSize.x-1),
		clamp(local.y,0,canvasSize.y-1)
		)
	
	return screenPos
	

#Drawing the brush stroke 
func drawBrush(pos: Vector2) -> void:
	var img = coloringLayers[currentPage]
	for x:int in range(-brushSize, brushSize+1):
		for y:int in range (-brushSize, brushSize+1):
			if x*x+ y*y < brushSize * brushSize:
				var px:int = int(pos.x)+x
				var py:int = int(pos.y)+y
				if (px >= 0 and
				 	px < int(canvasSize.x) and 
					py >= 0 and 
					py < int(canvasSize.y)):
						if lineMask.get_pixel(px,py).a<0.1:
							img.set_pixel(px,py,currentColor)
							print("painting at ", px, py)
							
							
	updateColorDisplay()
	

func startColoring(pos:Vector2) -> void:
	var local = getColoringPosition(pos)
	if drawingMode == "brush":
		saveState()
		isDrawing = true
		drawBrush(local)
	elif drawingMode == "bucket":
		saveState()
		floodFillScanline(local, currentColor)
		
		
func continueColoring(pos:Vector2)-> void:
	if not isDrawing:return
	drawBrush(getColoringPosition(pos))
	
func stopColoring()-> void:
	isDrawing = false
					
			
#Fill Tool
func floodFillScanline(startPos:Vector2, fillColor:Color)->void:
	var img:Image = coloringLayers[currentPage]
	var x:int = int(startPos.x)
	var y:int = int(startPos.y)
	
	if x < 0 or x >= canvasSize.x or y < 0 or y >= canvasSize.y:
		return
		
	var originalColor = img.get_pixel(x,y)
	if originalColor == fillColor:
		return
		
	var stack: Array[Vector2i] = [Vector2i(x,y)]
	var filledPixels: Dictionary = {}
	
	while stack.size() > 0:
		var current: Vector2i = stack.pop_back()
		var cx:int = current.x
		var cy:int = current.y
		
		if cx < 0 or cx >= canvasSize.x or cy < 0 or cy >= canvasSize.y:
			continue 
			
		var pixelKey:Vector2i = Vector2i(cx, cy)
		if filledPixels.has(pixelKey):
			continue 
			
		if img.get_pixel(cx, cy) != originalColor:
			continue
			
		if lineMask.get_pixel(cx, cy).a > 0.1:
			continue
			
		var left:int = cx
		var right:int = cx
		
		#left boundary
		while (
			left > 0 and 
		img.get_pixel(left-1, cy) == originalColor and 
		lineMask.get_pixel(left - 1,cy).a <= 0.1
		):
			left -= 1
		
		#right boundary	
		while (
			right < canvasSize.x-1 and 
		img.get_pixel(right+1, cy) == originalColor and 
		lineMask.get_pixel(right+ 1,cy).a <= 0.1
		):
			right += 1
			
		for i in range(left, right+1):
			img.set_pixel(i,cy,fillColor)
			filledPixels[Vector2i(i,cy)] = true
		
		for i:int in range(left,right+1):
			if cy > 0:
					var aboveKey = Vector2i(i, cy-1)
					if not (
						filledPixels.has(aboveKey) and 
						img.get_pixel(i, cy-1) == originalColor and 
						lineMask.get_pixel(i, cy-1).a <= 0.1):
							stack.append(Vector2i(i,cy-1))
			if cy < canvasSize.y -1:
				var belowKey:Vector2i = Vector2i(i,cy+1)
				if not (
						filledPixels.has(belowKey) and 
						img.get_pixel(i, cy+1) == originalColor and 
						lineMask.get_pixel(i, cy+1).a <= 0.1):
							stack.append(Vector2i(i,cy+1))
							
	updateColorDisplay()
	
#region Utility functions for buttons 
func setBurshColor(color:Color)-> void:
	currentColor = color
	
func setBrushSize(size:int)-> void:
	brushSize = size
	
func setDrawingMode(mode:String)-> void:
	drawingMode = mode
		

#region Linking Color Buttons
func _on_color_1_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)

func _on_color_2_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)


func _on_color_3_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)

func _on_color_4_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)


func _on_color_5_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)


func _on_color_6_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)


func _on_color_7_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)


func _on_color_8_pressed(source: BaseButton) -> void:
	currentColor = source.texture_normal.gradient.get_color(0)

#region brush settings
func _on_eraser_pressed() -> void:
	currentColor = Color.TRANSPARENT

func _on_bucket_pressed() -> void:
	setDrawingMode("bucket")

func _on_brush_pressed() -> void:
	setDrawingMode("brush")
	
func nextPage() -> void:
	if currentPage < pages.size()-1:
		currentPage += 1
		drawPage(currentPage)
		
func prevPage() -> void:
	if currentPage > 0:
		currentPage -= 1
		drawPage(currentPage)

func _on_next_pressed() -> void:
	nextPage()
	
func _on_previous_pressed() -> void:
	prevPage()
	
#Do not have button for this currently, 
#but one can be made if we want to offer
func clearPage() -> void:
	coloringLayers[currentPage].fill(Color.TRANSPARENT)
	updateColorDisplay()
	
	
func undo() -> void:
	if undoStack.is_empty():
		print("Nothing to undo")
		return
	var img:Image = undoStack.pop_back()
	var current:Image = coloringLayers[currentPage].duplicate()
	redoStack.append(current)
	coloringLayers[currentPage] = img	
	updateColorDisplay()
	
func redo() -> void:
	if redoStack.is_empty():
		print("Nothing to redo")
		return
	var img:Image = redoStack.pop_back()
	var current:Image = coloringLayers[currentPage].duplicate()
	undoStack.append(current)
	coloringLayers[currentPage] = img
	updateColorDisplay()
	
func  saveState() -> void:
	if currentPage < 0 or currentPage >= coloringLayers.size():
		push_warning("saveState: currentPage out of range: %s" % currentPage)
		return
	
	var img:Image = coloringLayers[currentPage]
	if img == null:
		push_warning("saveState: coloringLayers[%s] is null" % currentPage)
		return
		
	var snapshot = img.duplicate()
	undoStack.append(snapshot)
	redoStack.clear()
	
	if undoStack.size() > maxUndoSteps:
		undoStack.pop_front()

func _on_redo_pressed() -> void:
	redo()

func _on_undo_pressed() -> void:
	undo()
