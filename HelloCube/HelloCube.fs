module HelloCube

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL4
open OpenTK.Input
open SimpleGL

type Game() = 
    inherit GameWindow(853, 480, GraphicsMode.Default, "F# SimpleGL Sample")
    let vs = 
        VertexShader.Create 
            "C:\Users\Maik Klein\Documents\Visual Studio 2012\Projects\SimpleGL\HelloCube/vs.glsl"
    let fs = 
        FragmentShader.Create 
            "C:\Users\Maik Klein\Documents\Visual Studio 2012\Projects\SimpleGL\HelloCube/fs.glsl"
    let program = ShaderProgram.Create vs fs
    
    let attribute_vpos = ShaderProgram.GetAttribLocation "vPosition" program
    let attribute_vnorm =  ShaderProgram.GetAttribLocation "vNormal" program

    let view = 
        Matrix4.LookAt
            (Vector3(0.f, 0.f, 10.f), Vector3(0.f, 0.f, 0.f), 
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
        VAO.Create [ vbo_vertex; vbo_normal ] None
    
    let mesh = 
        { program = program
          ptype = PrimitiveType.Quads
          count = CubeData.vertdata.Length
          vao = vao }
    
    let mutable count = 0.0f
    
    override o.OnLoad e = 
        base.OnLoad(e)
        ShaderProgram.Use program
        //o.WindowBorder <- WindowBorder.Hidden
        // o.WindowState <- WindowState.Fullscreen
        let lightPos = Vector4.Transform(Vector4(2.0f, 10.0f, 8.0f, 1.0f), view)
        ShaderProgram.UniForm4 (ShaderProgram.GetUniformLocation "lightPos" program) lightPos 
        ShaderProgram.UniformMatrix4 (ShaderProgram.GetUniformLocation "proj" program) proj 
        ShaderProgram.UniformMatrix4 (ShaderProgram.GetUniformLocation "view" program) view 
        let mutable m = Matrix4.CreateRotationX(toRad count)
        let p = ShaderProgram.GetUniformLocation "model" program
        GL.UniformMatrix4(p, false, &m)
    
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
        if count > 360.f then count <- 0.0f
        count <- count + 1.0f
        let t = Matrix4.CreateTranslation(-3.0f, 0.0f, 0.0f)
        let t1 = Matrix4.CreateTranslation(3.0f, 0.0f, 0.0f)
        let t2 = Matrix4.CreateTranslation(0.0f, 3.0f, 0.0f)
        let t3 = Matrix4.CreateTranslation(0.0f, -3.0f, 0.0f)
        let t4 = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f)
        let r = Matrix4.CreateRotationY(toRad count)
        let r1 = Matrix4.CreateRotationX(toRad count)
        
        // let node2_2_1 = Node(RelRotation(t1,r),Some(mesh),[])
        // let node2_2   = Node(Translate(t3),Some(mesh),[node2_2_1])
        // let node2_1   = Node(Translate(t2),Some(mesh),[])
        // let node1_1   = Node(Rotation(r),Some(mesh),[])
        //let node1 = Node(Translate(t),None,[node1_1])
        //let node2 = Node(Translate(t1),None,
        //[node2_1;node2_2])
        let node1_1 = 
            { trans = Rotation(r)
              mesh = Some(mesh)
              nodeList = [] }
        
        let node1 = 
            { trans = RelRotation(t, r)
              mesh = None
              nodeList = [ node1_1 ] }
        
        let node2 = 
            { trans = RelRotation(t4, r1)
              mesh = Some(mesh)
              nodeList = [] }
        
        let scene = 
            { trans = Translate(t)
              mesh = Some(mesh)
              nodeList = [ node1; node2 ] }
        
        let l = genRenderMeshList Matrix4.Identity scene []
        renderMesh l
        showFps ((float32) o.RenderTime)
        base.SwapBuffers()
