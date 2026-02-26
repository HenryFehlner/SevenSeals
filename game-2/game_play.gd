extends CanvasLayer

@onready var coloringCanvas : Control = $ColoringCanvas
@onready var lineArtLayer:CanvasLayer = $LineArt
@onready var drawingContainer:Control = $LineArt/ColoringContainer

var currentPage:int

#can only put pictures in this array
var coloringLayers: Array[Image] = [

]

#will hold all the line art
var pages: Array[Image] = [
	
	preload("res://coloring-images/catipillar.jpg").get_image(),
	preload("res://coloring-images/stringray.jpg").get_image()
]

#size of the canvas, will be able to change it to mobile easily
var canvasSize: Vector2 = Vector2(400, 490)

var lineMask: Image

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	createPageDef()
	drawPage(0)


func createPageDef()-> void:
	coloringLayers.clear()
	#creates an specific page for each line art coloring oage.
	for i:int in range(pages.size()):
		#This will create each new page, size, and that it contains red, blue and yellow 
		var img: Image = Image.create(int(canvasSize.x),int(canvasSize.y),false,Image.FORMAT_RGBA8)
		img.fill(Color.TRANSPARENT)
		coloringLayers.append(img)
		

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
		

func updateColorDisplay() -> void:
	var display:TextureRect = coloringCanvas.get_node("ColoringLayer")
	if display:
		var text:ImageTexture = ImageTexture.new()
		text.set_image(coloringLayers[currentPage])
		display.texture = text
		
		
func drawPage(pageNumber:int) -> void:
	if pageNumber < 0 or pageNumber > pages.size():
		return
	currentPage = pageNumber
	#clearDrawingContainer()
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
	
