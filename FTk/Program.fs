module Program

open System
open System.Drawing
open System.Collections.Generic
open System.Text
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
let toRad x = x * (float32) System.Math.PI / 180.0f
let mutable mview = 
    Matrix4.LookAt
        (Vector3(0.f, 0.f, -10.f), Vector3(0.f, 0.f, 0.f), 
         Vector3(0.f, 1.f, 0.f)) 
    * Matrix4.CreatePerspectiveFieldOfView
          (toRad 60.0f, 4.0f / 3.0f, 0.1f, 100.0f)
let m = GL.GetUniformLocation(program.Handle, "mview")

let vertdata = 
    [| new Vector3(-1.0f, -1.0f, 0.0f)
       new Vector3(1.0f, -1.0f, 0.0f)
       new Vector3(0.0f, 1.0f, 0.0f) |]

let coldata = 
    [| new Vector3(1.0f, 1.0f, 0.0f)
       new Vector3(0.0f, 1.0f, 1.0f)
       new Vector3(1.0f, 0.0f, 1.0f) |]

let vao = 
    let vbo = 
        new VBO(BufferTarget.ArrayBuffer, 3, vertdata, attribute_vpos, 
                BufferUsageHint.StaticDraw)
    let vbo1 = 
        new VBO(BufferTarget.ArrayBuffer, 3, coldata, attribute_vcol, 
                BufferUsageHint.StaticDraw)
    new VAO([ vbo; vbo1 ])

type Game() = 
    inherit GameWindow(800, 600, GraphicsMode.Default, "F# OpenTK Sample")
    
    override o.OnLoad e = 
        base.OnLoad(e)
        program.Use()
        GL.UniformMatrix4(m, false, &mview)
    
    override o.OnResize e = 
        base.OnResize e
        GL.Enable(EnableCap.DepthTest)
        GL.Viewport
            (base.ClientRectangle.X, base.ClientRectangle.Y, 
             base.ClientRectangle.Width, base.ClientRectangle.Height)
    
    override o.OnUpdateFrame e = 
        base.OnUpdateFrame e
        if base.Keyboard.[Key.Escape] then base.Close()
    
    override o.OnRenderFrame(e) = 
        base.OnRenderFrame e
        GL.Clear
            (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        draw PrimitiveType.Triangles 0 3 vao
        base.SwapBuffers()