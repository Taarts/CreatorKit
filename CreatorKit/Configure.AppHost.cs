using CreatorKit.Extensions;
using Funq;
using CreatorKit.ServiceInterface;
using CreatorKit.ServiceModel;

[assembly: HostingStartup(typeof(CreatorKit.AppHost))]

namespace CreatorKit;

public class AppHost : AppHostBase, IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context,services) => {
            AppData.Set(context.Configuration);
            services.AddSingleton(AppData.Instance);
            services.AddSingleton<EmailRenderer>();
            services.AddPlugin(new CleanUrlsFeature());
            
            var scripts = InitOptions.ScriptContext; 
            scripts.ScriptAssemblies.Add(typeof(Hello).Assembly);
            scripts.ScriptMethods.Add(new ValidationScripts());
            scripts.Args[nameof(AppData)] = AppData.Instance;
        });

    public AppHost() : base("Creator Kit", typeof(MyServices).Assembly, typeof(CustomEmailServices).Assembly) {}

    public override void Configure(Container container)
    {
        SetConfig(new HostConfig {
            AddRedirectParamsToQueryString = true,
            UseSameSiteCookies = false,
            AllowFileExtensions = { "json" }
        });

        ConfigurePlugin<NativeTypesFeature>(x => 
            x.MetadataTypesConfig.GlobalNamespace = nameof(CreatorKit));
        
        MarkdownConfig.Transformer = new MarkdigTransformer();
        container.Resolve<AppData>().Load(this, 
            ContentRootDirectory.GetDirectory("emails"), RootDirectory.GetDirectory("img/mail"));
    }

    public static void RegisterLicense() =>
        Licensing.RegisterLicense("Individual (c) 2025 Patricia Amheiser SoVdCyFuKkkvbNrQ8qj5MUM35Eu3s5FBQ/ymbszdJogePSORfy0ey6mmDPCqsJXHbAdf+R6h7h0GPGBMM2QkhFfYdp1yftKGwxz6D/plP7rKuB1PBPYU4S0g6dMlk+Z89D0HSn+V/qul8WqrBJuo7zJGdJO6MXlMMMXYYbOEl1w=");
}
/* public static void RegisterLicense() =>
    Licensing.RegisterLicense(
        "Individual (c) 2025 Patricia Amheiser SoVdCyFuKkkvbNrQ8qj5MUM35Eu3s5FBQ/ymbszdJogePSORfy0ey6mmDPCqsJXHbAdf+R6h7h0GPGBMM2QkhFfYdp1yftKGwxz6D/plP7rKuB1PBPYU4S0g6dMlk+Z89D0HSn+V/qul8WqrBJuo7zJGdJO6MXlMMMXYYbOEl1w="
    );
 */

public class MarkdigTransformer : IMarkdownTransformer
{
    private Markdig.MarkdownPipeline Pipeline { get; } = 
        Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();
    public string Transform(string markdown) => Markdig.Markdown.ToHtml(markdown, Pipeline);
}

