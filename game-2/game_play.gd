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
var drawingMode: String = "brush"
var undoStack: Array[Image] = []
var redoStack: Array[Image] = []
var UndoSteps: int = 200

#region Page Data
#can only put pictures in this array
var coloringLayers: Array[Image] = []
#will hold all the line art
var pages: Array[Image] = [
	
	preload("res://coloring-images/pikachu2.png").get_image(),
	preload("res://coloring-images/stringray.jpg").get_image()
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
	if pageNumber < 0 or pageNumber > pages.size():
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
		isDrawing = true
		drawBrush(local)
		
func continueColoring(pos:Vector2)-> void:
	if not isDrawing:return
	drawBrush(getColoringPosition(pos))
	
func stopColoring()-> void:
	isDrawing = false
					
			
