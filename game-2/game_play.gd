extends CanvasLayer

@onready var coloringCanvas: Control = $ColoringCanvas
@onready var lineArtLayer: CanvasLayer = $LineArt
@onready var drawingContainer: Control = $LineArt/ColoringContainer
@onready var colorToolbar: Control = $ColorToolbar
@onready var paletteButtons = $ColorToolbar/GridContainer.get_children()

var currentColor: Color = Color.WHITE
var drawingMode: String = "bucket"

var undoStack: Array[Image] = []
var redoStack: Array[Image] = []
var maxUndoSteps: int = 100
var floodFillActive: bool = false

var coloringLayer: Image
var activeLineArt: Image
var lineMask: Image
var regionMap: Image
var previewLayer: Image

var canvasSize: Vector2 = Vector2(400, 490)
var previewOpacity: float = 0.22

var activePainting
var activePaintingKey: String = ""

# hidden region-map color -> correct paint color
var region_to_required_color: Dictionary = {}


func _ready() -> void:
	activePainting = GlobalData.GetActivePainting()

	if activePainting == null:
		push_error("No active painting found in GlobalData.")
		return

	load_active_painting()
	setup_region_rules()
	build_preview_layer()
	draw_active_page()

	# Leave toolbar visible for player-picked colors
	setupPlayerPalette()

	setDrawingMode("bucket")


# =========================================================
# ACTIVE PAINTING
# =========================================================

func load_active_painting() -> void:
	var image_path: String = str(activePainting.ColoringImagePath)
	var texture = load(image_path)

	if texture == null:
		push_error("Could not load image: " + image_path)
		return

	# Use image filename instead of painting id to avoid capitalization mismatch
	activePaintingKey = image_path.get_file().get_basename().to_lower()
	print("Active painting key from image path: ", activePaintingKey)

	activeLineArt = texture.get_image()
	activeLineArt.convert(Image.FORMAT_RGBA8)
	activeLineArt.resize(int(canvasSize.x), int(canvasSize.y))

	lineMask = activeLineArt.duplicate()

	load_region_map(image_path)

	coloringLayer = Image.create(int(canvasSize.x), int(canvasSize.y), false, Image.FORMAT_RGBA8)
	coloringLayer.fill(Color.TRANSPARENT)


func load_region_map(line_art_path: String) -> void:
	var region_path = line_art_path.get_basename() + "_regions.png"
	var region_texture = load(region_path)

	if region_texture == null:
		push_error("Could not load region map: " + region_path)
		return

	regionMap = region_texture.get_image()
	regionMap.convert(Image.FORMAT_RGBA8)
	regionMap.resize(int(canvasSize.x), int(canvasSize.y))


func build_preview_layer() -> void:
	previewLayer = Image.create(int(canvasSize.x), int(canvasSize.y), false, Image.FORMAT_RGBA8)
	previewLayer.fill(Color.TRANSPARENT)

	if regionMap == null:
		return

	for y in range(int(canvasSize.y)):
		for x in range(int(canvasSize.x)):
			var region_color: Color = get_clean_color(regionMap.get_pixel(x, y))

			if region_color.a < 0.1:
				continue

			var required_color: Color = get_required_color_for_region(region_color)

			if required_color == Color.TRANSPARENT:
				continue

			previewLayer.set_pixel(
				x,
				y,
				Color(required_color.r, required_color.g, required_color.b, previewOpacity)
			)


func draw_active_page() -> void:
	clearDrawingContainer()
	clearColoringCanvas()

	# Faded correct-color preview underneath
	if previewLayer != null:
		var preview_tex := ImageTexture.create_from_image(previewLayer)
		var preview_rect := TextureRect.new()
		preview_rect.texture = preview_tex
		preview_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		preview_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		preview_rect.name = "PreviewLayer"
		preview_rect.z_index = 0
		drawingContainer.add_child(preview_rect)

	# Player color layer
	var color_rect := TextureRect.new()
	color_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	color_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	color_rect.name = "ColoringLayer"
	color_rect.z_index = 1
	coloringCanvas.add_child(color_rect)

	# Line art on top
	var line_tex := ImageTexture.create_from_image(activeLineArt)
	var line_rect := TextureRect.new()
	line_rect.texture = line_tex
	line_rect.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	line_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	line_rect.name = "LineArtImage"
	line_rect.z_index = 2
	drawingContainer.add_child(line_rect)

	updateColorDisplay()
	lineArtLayer.layer = 1


func clearDrawingContainer() -> void:
	for child in drawingContainer.get_children():
		child.queue_free()


func clearColoringCanvas() -> void:
	for child in coloringCanvas.get_children():
		child.queue_free()


func updateColorDisplay() -> void:
	var display: TextureRect = coloringCanvas.get_node_or_null("ColoringLayer")
	if display != null:
		var tex := ImageTexture.create_from_image(coloringLayer)
		display.texture = tex


# =========================================================
# PLAYER PALETTE (TOP TOOLBAR)
# =========================================================

func setupPlayerPalette() -> void:
	var saved_colors = GlobalData.GetSavedColors()

	for i in range(paletteButtons.size()):
		if i < saved_colors.size():
			paletteButtons[i].texture_normal = make_color_texture(saved_colors[i])
			paletteButtons[i].disabled = false
		else:
			paletteButtons[i].texture_normal = make_color_texture(Color(0.35, 0.35, 0.35, 1))
			paletteButtons[i].disabled = true


func refreshPlayerPalette() -> void:
	setupPlayerPalette()


func setPaletteColor(index: int) -> void:
	var saved_colors = GlobalData.GetSavedColors()

	if index >= 0 and index < saved_colors.size():
		currentColor = saved_colors[index]
		print("Selected player-picked color: ", currentColor)


func _on_color_1_pressed() -> void:
	setPaletteColor(0)

func _on_color_2_pressed() -> void:
	setPaletteColor(1)

func _on_color_3_pressed() -> void:
	setPaletteColor(2)

func _on_color_4_pressed() -> void:
	setPaletteColor(3)

func _on_color_5_pressed() -> void:
	setPaletteColor(4)

func _on_color_6_pressed() -> void:
	setPaletteColor(5)

func _on_color_7_pressed() -> void:
	setPaletteColor(6)

func _on_color_8_pressed() -> void:
	setPaletteColor(7)


func make_color_texture(color: Color) -> ImageTexture:
	var img := Image.create(32, 32, false, Image.FORMAT_RGBA8)
	img.fill(color)
	return ImageTexture.create_from_image(img)


# =========================================================
# INPUT
# =========================================================

func _input(event: InputEvent) -> void:
	if floodFillActive:
		return

	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if isMouseOverUi(event.position):
				return

			if event.pressed:
				startColoring(event.position)


func isMouseOverUi(mousePos: Vector2) -> bool:
	if $ToolBar and $ToolBar.get_global_rect().has_point(mousePos):
		return true
	if colorToolbar and colorToolbar.get_global_rect().has_point(mousePos):
		return true
	return false


func getColoringPosition(screenPos: Vector2) -> Vector2:
	var textRect = drawingContainer.get_node_or_null("LineArtImage")
	if textRect == null:
		return Vector2.ZERO

	var imgSize: Vector2 = textRect.texture.get_size()
	var rectSize: Vector2 = textRect.size
	var cScale = min(rectSize.x / imgSize.x, rectSize.y / imgSize.y)
	var cOffset = (rectSize - imgSize * cScale) / 2.0
	var local = (screenPos - textRect.global_position - cOffset) / cScale

	return Vector2(
		clamp(local.x, 0, canvasSize.x - 1),
		clamp(local.y, 0, canvasSize.y - 1)
	)


func startColoring(pos: Vector2) -> void:
	if drawingMode != "bucket":
		return

	if regionMap == null:
		push_error("regionMap is null. You need the hidden _regions image.")
		return

	var local := getColoringPosition(pos)
	var x := int(local.x)
	var y := int(local.y)

	if not is_inside_canvas(x, y):
		return

	var clicked_region_color: Color = get_clean_color(regionMap.get_pixel(x, y))

	if clicked_region_color.a < 0.1:
		return

	var required_color: Color = get_required_color_for_region(clicked_region_color)

	if required_color == Color.TRANSPARENT:
		print("No required color mapped to this region.")
		return

	# Player must have picked/unlocked this color already
	if not player_has_unlocked_color(required_color):
		print("Player has not unlocked this color yet.")
		return

	# Player must also currently have that exact color selected in the toolbar
	if not colors_match(currentColor, required_color):
		print("Wrong color selected for this region.")
		return

	saveState()
	floodFillRegionByMap(clicked_region_color, currentColor)


func is_inside_canvas(x: int, y: int) -> bool:
	return x >= 0 and x < int(canvasSize.x) and y >= 0 and y < int(canvasSize.y)


# =========================================================
# REGION RULES
# =========================================================

func setup_region_rules() -> void:
	region_to_required_color.clear()

	match activePaintingKey:

		"pikachu2":
			
			# RED -> body
			region_to_required_color[color_key(Color(1, 0, 0, 1))] = Color(0.991, 0.832, 0.0, 1.0)
			
			# GREEN -> background
			region_to_required_color[color_key(Color(0, 1, 0, 1))] = Color(0.689, 0.877, 0.961, 1.0)
			
			# MAGENTA -> cheeks
			region_to_required_color[color_key(Color(1, 0, 1, 1))] = Color(0.962, 0.382, 0.561, 1.0)

			# CYAN -> highlights
			region_to_required_color[color_key(Color(0, 1, 1, 1))] = Color(0.922, 0.769, 0.991, 1.0)
			
			# YELLOW -> base tail
			region_to_required_color[color_key(Color(1, 1, 0, 1))] = Color(0.592, 0.349, 0.231, 1.0)


		"mareanie":
			# RED -> teal outer body
			region_to_required_color[color_key(Color(1, 0, 0, 1))] = Color(0.39, 0.676, 0.704, 1.0)

			# GREEN -> purple inner body
			region_to_required_color[color_key(Color(0, 1, 0, 1))] = Color(0.615, 0.41, 0.802, 1.0)

			# BLUE -> light blue eyes
			region_to_required_color[color_key(Color(0, 0, 1, 1))] = Color(0.418, 0.773, 0.95, 1.0)

			# YELLOW -> crown
			region_to_required_color[color_key(Color(1, 1, 0, 1))] = Color(0.881, 0.819, 0.404, 1.0)

			# MAGENTA -> spikes
			region_to_required_color[color_key(Color(1, 0, 1, 1))] = Color(0.80, 0.70, 0.90, 1.0)

			# CYAN -> tongue
			region_to_required_color[color_key(Color(0, 1, 1, 1))] = Color(1.0, 0.55, 0.663, 1.0)

			# BLACK -> mouth
			region_to_required_color[color_key(Color(0, 0, 0, 1))] = Color(0.0, 0.0, 0.0, 1.0)


		"slyveon":
			# RED -> light gray
			region_to_required_color[color_key(Color(1, 0, 0, 1))] = Color(0.85, 0.85, 0.85, 1)

			# BLUE -> white
			region_to_required_color[color_key(Color(0, 0, 1, 1))] = Color(1, 1, 1, 1)

			# GREEN -> pink
			region_to_required_color[color_key(Color(0, 1, 0, 1))] = Color(1.0, 0.651, 0.822, 1.0)

			# CYAN -> light blue
			region_to_required_color[color_key(Color(0, 1, 1, 1))] = Color(0.651, 0.825, 1.0, 1.0)

			# YELLOW -> dark blue
			region_to_required_color[color_key(Color(1, 1, 0, 1))] = Color(0.358, 0.489, 0.947, 1.0)

			# MAGENTA -> light purple
			region_to_required_color[color_key(Color(1, 0, 1, 1))] = Color(0.8, 0.7, 0.9, 1)


		_:
			print("No region rules set for painting key: ", activePaintingKey)


func get_required_color_for_region(region_color: Color) -> Color:
	var key = color_key(region_color)
	if region_to_required_color.has(key):
		return region_to_required_color[key]
	return Color.TRANSPARENT


func color_key(c: Color) -> String:
	var rc = int(round(c.r * 255.0))
	var gc = int(round(c.g * 255.0))
	var bc = int(round(c.b * 255.0))
	var ac = int(round(c.a * 255.0))
	return str(rc) + "_" + str(gc) + "_" + str(bc) + "_" + str(ac)


func get_clean_color(c: Color) -> Color:
	return Color(
		round(c.r * 255.0) / 255.0,
		round(c.g * 255.0) / 255.0,
		round(c.b * 255.0) / 255.0,
		round(c.a * 255.0) / 255.0
	)


func colors_match(a: Color, b: Color) -> bool:
	return color_key(get_clean_color(a)) == color_key(get_clean_color(b))


# =========================================================
# FILL
# =========================================================

func floodFillRegionByMap(target_region_color: Color, fill_color: Color) -> void:
	if regionMap == null or coloringLayer == null:
		return

	floodFillActive = true

	var width := int(canvasSize.x)
	var height := int(canvasSize.y)

	for y in range(height):
		for x in range(width):
			var region_color_here = get_clean_color(regionMap.get_pixel(x, y))
			if colors_match(region_color_here, target_region_color):
				coloringLayer.set_pixel(x, y, fill_color)

	floodFillActive = false
	updateColorDisplay()


# =========================================================
# TOOL BUTTONS
# =========================================================

func setDrawingMode(mode: String) -> void:
	drawingMode = mode.to_lower()


func _on_bucket_pressed() -> void:
	setDrawingMode("bucket")


func _on_eraser_pressed() -> void:
	print("Eraser is disabled in paint-by-number mode.")


# =========================================================
# UNDO / REDO
# =========================================================

func saveState() -> void:
	if coloringLayer == null:
		return

	var snapshot = coloringLayer.duplicate()
	undoStack.append(snapshot)
	redoStack.clear()

	if undoStack.size() > maxUndoSteps:
		undoStack.pop_front()


func undo() -> void:
	if undoStack.is_empty():
		print("Nothing to undo")
		return

	var previous: Image = undoStack.pop_back()
	var current: Image = coloringLayer.duplicate()
	redoStack.append(current)
	coloringLayer = previous
	updateColorDisplay()


func redo() -> void:
	if redoStack.is_empty():
		print("Nothing to redo")
		return

	var next_img: Image = redoStack.pop_back()
	var current: Image = coloringLayer.duplicate()
	undoStack.append(current)
	coloringLayer = next_img
	updateColorDisplay()


func _on_undo_pressed() -> void:
	undo()


func _on_redo_pressed() -> void:
	redo()


# =========================================================
# HELPERS
# =========================================================

func player_has_unlocked_color(check_color: Color) -> bool:
	var saved_colors = GlobalData.GetSavedColors()

	for c in saved_colors:
		if colors_match(c, check_color):
			return true

	return false


# =========================================================
# NAV
# =========================================================

func _on_gallery_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/image-gallery.tscn")


func _on_camera_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/CameraTest.tscn")


func _on_painting_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/painting-gallery.tscn")
