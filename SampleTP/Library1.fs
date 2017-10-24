namespace ProviderImplementation

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System
open System.Collections.Generic
open System.Reflection

[<TypeProvider>]
/// [omit]
type public SampleTypeProvider(config : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces(config)

    let namespaceName = "Sample"
    let thisAssembly = Assembly.GetExecutingAssembly()
    let sampleTpType = ProvidedTypeDefinition(thisAssembly, namespaceName, "SampleTypeProvider", baseType = Some typeof<obj>)
            
    let buildTypes (typeName : string) (args : obj []) = 
        // Create the top level property
        let typeProviderForAccount = ProvidedTypeDefinition(thisAssembly, namespaceName, typeName, baseType = Some typeof<obj>)
        typeProviderForAccount.AddMember(ProvidedConstructor([], fun _ -> <@@ null @@>))
        
        let domainTypes = ProvidedTypeDefinition("Domain", Some typeof<obj>)

        // Now create child members.
        typeProviderForAccount.AddMember (TypeFactory.getBlobStorageMembers domainTypes)
        typeProviderForAccount.AddMember domainTypes

        typeProviderForAccount
    
    let createParam (name, defaultValue:'a, help) =
        let providedParameter = ProvidedStaticParameter(name, typeof<'a>, defaultValue)
        providedParameter.AddXmlDoc help
        providedParameter
    
    // Parameterising the provider
    let parameters = [ createParam("theParam", String.Empty, "The Test Parameter") ]
    
    let memoize func =
        let cache = Dictionary()
        fun argsAsString args ->
            if not (cache.ContainsKey argsAsString) then
                cache.Add(argsAsString, func argsAsString args)
            cache.[argsAsString]

    do
        sampleTpType.DefineStaticParameters(parameters, memoize buildTypes)
        this.AddNamespace(namespaceName, [ sampleTpType ])
        sampleTpType.AddXmlDoc("The sample type.")

[<TypeProviderAssembly>]
do ()