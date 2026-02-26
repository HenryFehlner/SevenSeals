extends CanvasLayer

#can only put pictures in this array
var coloringLayers: Array[Image] = [
preload("res://coloring-images/catipillar.jpg").get_image(),
preload("res://coloring-images/stringray.jpg").get_image()
]

#will hold all the line art
var pages: Array[Image] = []

#size of the canvas, will be able to change it to mobile easily
var canvasSize: Vector2 = Vector2(800, 600)

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	createPageDef()


func createPageDef()-> void:
	#creates an specific page for each line art coloring oage.
	for i:int in range(pages.size()):
		#This will create each new page, size, and that it contains red, blue and yellow 
		var img: Image = Image.create(int(canvasSize.x),int(canvasSize.y),false,Image.FORMAT_RGBA8)
		img.fill(Color.TRANSPARENT)
		coloringLayers.append(img)
		
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
