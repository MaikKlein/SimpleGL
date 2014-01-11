module HelloCube

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open SimpleGL


let mutable count = 0.0f

type Game() = 
    inherit GameWindow(1920 , 1080, GraphicsMode.Default, "F# SimpleGL Sample")
    
    let vs = VertexShader.Create "../../vs.glsl"
    let fs = FragmentShader.Create "../../fs.glsl"
    
    let program = ShaderProgram(vs,fs)
    
    let attribute_vpos = program.GetAttribLocation "vPosition"
    let attribute_vnorm = program.GetAttribLocation "vNormal"
    
    let view = 
        Matrix4.LookAt
            (Vector3(0.f, 0.f, -5.f), Vector3(0.f, 0.f, 0.f), 
             Vector3(0.f, 1.f, 0.f)) 
    
    let proj =
        Matrix4.CreatePerspectiveFieldOfView
            (toRad 45.0f, 16.0f / 9.0f, 0.1f, 100.0f)
    
    let vao = 
        let vbo_vertex = 
            VBO.from 
                (BufferTarget.ArrayBuffer, 3, CubeData.vertdata, attribute_vpos, 
                 BufferUsageHint.StaticDraw, false)
        let vbo_normal =
            VBO.from
                (BufferTarget.ArrayBuffer,3,CubeData.ndata,attribute_vnorm,
                 BufferUsageHint.StaticDraw, false)
    
        new VAO([ vbo_vertex; vbo_normal ])

    override o.OnLoad e = 
        base.OnLoad(e)
        program.Use()
        o.WindowBorder <- WindowBorder.Hidden
        o.WindowState <- WindowState.Fullscreen 
        let lightPos = Vector4.Transform(Vector4(0.0f,5.0f,-5.0f,1.0f),view)
        program.UniForm4 (program.GetUniformLocation "lightPos") (lightPos)

    override o.OnResize e = 
        base.OnResize e
        GL.Enable(EnableCap.DepthTest)
        GL.Viewport
            (base.ClientRectangle.X, base.ClientRectangle.Y, 
             base.ClientRectangle.Width, base.ClientRectangle.Height)
    
    override o.OnUpdateFrame e = 
        base.OnUpdateFrame e
        if base.Keyboard.[Key.Escape] then base.Close()
        let model = Matrix4.CreateRotationY(toRad count) 
                    * Matrix4.CreateRotationX(toRad count)
        count <- count + 1.0f 

        let normalMatrix = Matrix4.Transpose(Matrix4.Invert(model)) * view
        program.UniformMatrix4 (program.GetUniformLocation "normalMatrix") normalMatrix
        let modelView = model * view
        program.UniformMatrix4 (program.GetUniformLocation "modelView") modelView
        program.UniformMatrix4 (program.GetUniformLocation "mvp") (modelView * proj)
    
    override o.OnRenderFrame(e) = 
        base.OnRenderFrame e
        GL.Clear
            (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        draw(PrimitiveType.Quads,0,CubeData.vertdata.Length,vao)
        base.SwapBuffers()
