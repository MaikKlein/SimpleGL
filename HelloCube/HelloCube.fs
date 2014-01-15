module HelloCube

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open SimpleGL

let mutable count = 0.0f

type Game() = 
    inherit GameWindow(640, 360, new GraphicsMode(ColorFormat.Empty, 32, 32, 4), 
                       "F# SimpleGL Sample")
    let vs = VertexShader.Create "../../vs.glsl"
    let fs = FragmentShader.Create "../../fs.glsl"
    let program = ShaderProgram(vs, fs)
    let attribute_vpos = program.GetAttribLocation "vPosition"
    let attribute_vnorm = program.GetAttribLocation "vNormal"
    let view = 
        Matrix4.LookAt
            (Vector3(0.f, 0.f, -10.f), Vector3(0.f, 0.f, 0.f), 
             Vector3(0.f, 1.f, 0.f))
    let proj = 
        Matrix4.CreatePerspectiveFieldOfView
            (toRad 60.0f, 16.0f / 9.0f, 0.1f, 100.0f)
    
    let vao = 
        let vbo_vertex = 
            VBO.from 
                (BufferTarget.ArrayBuffer, 3, CubeData.vertdata, attribute_vpos, 
                 BufferUsageHint.StaticDraw, false)
        let vbo_normal = 
            VBO.from 
                (BufferTarget.ArrayBuffer, 3, CubeData.ndata, attribute_vnorm, 
                 BufferUsageHint.StaticDraw, false)
        new VAO([ vbo_vertex; vbo_normal ])
    
    let mesh = 
        { program = program
          ptype = PrimitiveType.Quads
          count = CubeData.vertdata.Length
          vao = vao }
    
    
    override o.OnLoad e = 
        base.OnLoad(e)
        program.Use()
        o.WindowBorder <- WindowBorder.Hidden
        o.WindowState <- WindowState.Fullscreen
        let lightPos = 
            Vector4.Transform(Vector4(2.0f, 10.0f, -8.0f, 1.0f), view)
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
        program.UniformMatrix4 (program.GetUniformLocation "proj") proj
        program.UniformMatrix4 (program.GetUniformLocation "view") view
    
    override o.OnRenderFrame(e) = 
        base.OnRenderFrame e
        GL.Clear
            (ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        let model = 
            Matrix4.CreateRotationY(toRad count) 
            * Matrix4.CreateRotationX(toRad count)
        count <- count + 1.0f
        if count > 360.f then count <- 0.0f

        let t = Matrix4.CreateTranslation(-3.0f,0.0f,0.0f)
        let t1 = Matrix4.CreateTranslation(3.0f,0.0f,0.0f)
        let t2 = Matrix4.CreateTranslation(0.0f,3.0f,0.0f)
        let t3 = Matrix4.CreateTranslation(0.0f,-3.0f,0.0f)
        
        let r = Matrix4.CreateRotationY(toRad count)


        let node2_2_1 = Node(RelRotation(t1,r),Some(mesh),[])
        let node2_2 = Node(Translate(t3),Some(mesh),[node2_2_1])
        let node2_1 = Node(Translate(t2),Some(mesh),[])
        let node1_1 = Node(Rotation(r),Some(mesh),[])

        let node1 = Node(Translate(t),None,[node1_1])
        let node2 = Node(Translate(t1),None,
                         [node2_1;node2_2])
       
        let scene = Node(Id, None, [ node1; node2 ])
        
        renderScene Matrix4.Identity scene
        base.SwapBuffers()
