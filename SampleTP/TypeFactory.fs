module TypeFactory

open ProviderImplementation.ProvidedTypes

let x = 1
let private createContainerType (domainType : ProvidedTypeDefinition) name = 
    let individualContainerType = ProvidedTypeDefinition("StaticProp", Some typeof<obj>, hideObjectMethods = true)
    individualContainerType.AddXmlDoc <| sprintf "Provides access to a static property." 
    domainType.AddMember individualContainerType
    // this local binding is required for the quotation.
    let containerName = name
    let containerProp = ProvidedProperty(containerName, individualContainerType, getterCode = fun _ -> <@@ containerName @@>)
    containerProp.AddXmlDocDelayed(fun () -> sprintf "Provides access to the %s property off the static property." containerName)
    containerProp

let getBlobStorageMembers (domainType : ProvidedTypeDefinition) = 
    let containerListingType = ProvidedTypeDefinition("Containers", Some typeof<obj>, hideObjectMethods = true)
    let createContainerType = createContainerType domainType
    
    (* This line goes pop when consuming the TP e.g.
    
       #r @"SampleTP.dll"
       open SampleTP
       type Foo = Sample.SampleTypeProvider<"Test">
       Foo.StaticProperty.A
    
       Replacing it with the following works.
    
       containerListingType.AddMembers (["A";"B";"C";"D" ] |> List.map createContainerType)
    
    *)
    containerListingType.AddMembersDelayed(fun _ -> ["A";"B";"C";"D" ] |> List.map createContainerType)
    
    domainType.AddMember containerListingType

    let containerListingProp = ProvidedProperty("StaticProperty", containerListingType, isStatic = true, getterCode = (fun _ -> <@@ () @@>))
    containerListingProp.AddXmlDoc "A static property."
    containerListingProp