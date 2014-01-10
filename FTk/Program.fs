module Program

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open SimpleGL

let vs = Shader("../../vs.glsl", ShaderType.VertexShader)
let fs = Shader("../../fs.glsl", ShaderType.FragmentShader)

let program = ShaderProgram([ vs; fs ])

let attribute_vpos = program.GetAttribLocation "vPosition"
let attribute_vcol = program.GetAttribLocation "vColor"
let attribute_vOff = program.GetAttribLocation "vOffset"

let toRad x = x * (float32) System.Math.PI / 180.0f

let mutable view = 
    Matrix4.LookAt
        (Vector3(0.f, 0.f, -15.f), Vector3(0.f, 0.f, 0.f), 
         Vector3(0.f, 1.f, 0.f)) 

let mutable proj =
    Matrix4.CreatePerspectiveFieldOfView
        (toRad 45.0f, 4.0f / 3.0f, 0.1f, 100.0f)

let mutable model = Matrix4.Identity

let coldata = 
    [| new Vector3(1.0f, 0.0f, 0.0f)
       new Vector3(0.0f, 1.0f, 0.0f)
       new Vector3(0.0f, 0.0f, 1.0f)
       new Vector3(1.0f, 1.0f, 0.0f)
       new Vector3(1.0f, 0.0f, 0.0f)
       new Vector3(0.0f, 1.0f, 0.0f)
       new Vector3(0.0f, 0.0f, 1.0f)
       new Vector3(1.0f, 1.0f, 0.0f) |]

let vertdata = 
    [| new Vector3(1.0f, 1.0f, 1.0f)
       new Vector3(-1.0f, 1.0f, 1.0f)
       new Vector3(-1.0f, -1.0f, 1.0f)
       new Vector3(1.0f, -1.0f, 1.0f)
       new Vector3(1.0f, 1.0f, -1.0f)
       new Vector3(-1.0f, 1.0f, -1.0f)
       new Vector3(-1.0f, -1.0f, -1.0f)
       new Vector3(1.0f, -1.0f, -1.0f) |]
let index = 
    [| 0; 1; 2; 3; 7; 6; 5; 4; 0; 1; 5; 4; 2; 3; 7; 6; 0; 4; 7; 3; 1; 5; 6; 2 |]

let offset = 
    [| new Vector3(-2.0f, 0.0f, 0.0f)
       new Vector3(-4.0f, 0.0f, 0.0f)
       new Vector3(-6.0f, 0.0f, 0.0f) |]

let vao = 
    let vbo_vertex = 
        VBO.from 
            (BufferTarget.ArrayBuffer, 3, vertdata, attribute_vpos, 
             BufferUsageHint.StaticDraw, false)
    let vbo_color = 
        VBO.from 
            (BufferTarget.ArrayBuffer, 3, coldata, attribute_vcol, 
             BufferUsageHint.StaticDraw, false)

    let ibo = IBO(index, BufferUsageHint.StaticDraw)
    new VAO([ vbo_vertex; vbo_color; ibo ])

let mutable count = 0.0f

type Game() = 
    inherit GameWindow(800, 600, GraphicsMode.Default, "F# SimpleGL Sample")
    
    override o.OnLoad e = 
        base.OnLoad(e)
        program.Use()
        program.UniformMatrix4 (program.GetUniformLocation "view") view
        program.UniformMatrix4 (program.GetUniformLocation "proj") proj
    override o.OnResize e = 
        base.OnResize e
        GL.Enable(EnableCap.DepthTest)
        GL.Viewport
            (base.ClientRectangle.X, base.ClientRectangle.Y, 
             base.ClientRectangle.Width, base.ClientRectangle.Height)
    
    override o.OnUpdateFrame e = 
        base.OnUpdateFrame e
        if base.Keyboard.[Key.Escape] then base.Close()
        model <- Matrix4.CreateRotationY(count) 
        count <- count + 1.0f * (float32)o.RenderTime
        program.UniformMatrix4 (program.GetUniformLocation "model") model
    
    override o.OnRenderFrame(e) = 
        base.OnRenderFrame e
        GL.Clear
            (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        indexDraw 
            (BeginMode.Quads, index.Length, DrawElementsType.UnsignedInt, vao)
        base.SwapBuffers()
