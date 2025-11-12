using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MenuLib.MonoBehaviors;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MenuLib;

[BepInPlugin("nickklmao.menulib", MOD_NAME, "2.6.0")]
internal sealed class Entry : BaseUnityPlugin
{
    private const string MOD_NAME = "Menu Lib";
        
    internal static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(MOD_NAME);
        
    private static void MenuPageMain_StartHook(Action<MenuPageMain> orig, MenuPageMain self)
    {
        orig.Invoke(self);
        MenuAPI.mainMenuBuilderDelegate?.Invoke(self.transform);
    }
        
    private static void MenuPageSettings_StartHook(Action<MenuPageSettings> orig, MenuPageSettings self)
    {
        orig.Invoke(self);
        MenuAPI.settingsMenuBuilderDelegate?.Invoke(self.transform);
    }
        
    private static void MenuPageColor_StartHook(Action<MenuPageColor> orig, MenuPageColor self)
    {
        orig.Invoke(self);
        MenuAPI.colorMenuBuilderDelegate?.Invoke(self.transform);
    }
        
    private static void MenuPageEsc_StartHook(Action<MenuPageEsc> orig, MenuPageEsc self)
    {
        orig.Invoke(self);
        MenuAPI.escapeMenuBuilderDelegate?.Invoke(self.transform);
    }
    
    private static void MenuPageRegions_StartHook(Action<MenuPageRegions> orig, MenuPageRegions self)
    {
        orig.Invoke(self);
        MenuAPI.regionSelectionMenuBuilderDelegate?.Invoke(self.transform);
    }
    
    private static void MenuPageServerList_StartHook(Action<MenuPageServerList> orig, MenuPageServerList self)
    {
        orig.Invoke(self);
        MenuAPI.serverListMenuBuilderDelegate?.Invoke(self.transform);
    }
        
    private static void MenuPageLobby_StartHook(Action<MenuPageLobby> orig, MenuPageLobby self)
    {
        orig.Invoke(self);
        MenuAPI.lobbyMenuBuilderDelegate?.Invoke(self.transform);
    }
        
    private static void SemiFunc_UIMouseHoverILHook(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instruction => instruction.MatchBrfalse(out var label) && label.Target.OpCode == OpCodes.Ldarg_1);

        cursor.Index += 2;
        cursor.RemoveRange(27);

        cursor.Emit(OpCodes.Ldloc_0);
        cursor.EmitDelegate((MenuScrollBox menuScrollBox, Vector2 vector) =>
        {
            var mask = (RectTransform) menuScrollBox.scroller.parent;

            var bottom = mask.position.y;
            var top = bottom + mask.sizeDelta.y;

            return vector.y > bottom && vector.y < top;
        });

        var jumpToLabel = cursor.DefineLabel();

        cursor.Emit(OpCodes.Brtrue_S, jumpToLabel);
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.Ret);

        cursor.MarkLabel(jumpToLabel);
    }
        
    private static void MenuPage_StateClosingILHook(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(instruction => instruction.MatchLdfld<MenuPage>("stateStart"));

        cursor.Index += 2;
            
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((MenuPage menuPage) =>
        {
            if (!MenuAPI.customMenuPages.TryGetValue(menuPage, out var repoPopupPage))
                return;
                
            var rectTransform = (RectTransform) menuPage.transform;

            var animateAwayPosition = (Vector2) rectTransform.position;
            animateAwayPosition.y = -rectTransform.rect.height - repoPopupPage.rectTransform.rect.height;

            REPOReflection.menuPage_AnimateAwayPosition.SetValue(menuPage, animateAwayPosition);
        });
            
        cursor.GotoNext(instruction => instruction.MatchCall<Object>("Destroy"));

        cursor.Index -= 5;
        cursor.RemoveRange(6);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((MenuPage menuPage) =>
        {
            if (MenuAPI.customMenuPages.TryGetValue(menuPage, out var repoPopupPage) && (repoPopupPage.isCachedPage || !repoPopupPage.pageWasActivatedOnce))
            {
                menuPage.enabled = false;
                return;
            }
                
            MenuManager.instance.PageRemove(menuPage);
            Destroy(menuPage.gameObject);
        });
    }

    private static void MenuScrollBox_UpdateILHook(ILContext il)
    {
        var cursor = new ILCursor(il);

        //cursor.GotoNext(instruction => instruction.MatchLdarg(0), instruction => instruction.MatchLdfld<MenuScrollBox>("scrollBoxActive"));
            
        //cursor.RemoveRange(4);

        //var returnLabel = cursor.MarkLabel();

        //cursor.Index -= 2;
        //cursor.Remove();
        //cursor.Emit(OpCodes.Brtrue_S, returnLabel);

        //cursor.GotoNext(instruction => instruction.MatchCall(typeof(SemiFunc), "InputScrollY"));
            
        //cursor.Index--;
        //cursor.Remove();
            
        //var customScrollLogicLabel = cursor.DefineLabel();
        //cursor.Emit(OpCodes.Bne_Un_S, customScrollLogicLabel);

        //cursor.Index += 2;
            
        //var postIfLabel = il.Instrs[cursor.Index].Operand as ILLabel;
            
        //cursor.Index++;
        //cursor.RemoveRange(24);
        //cursor.MarkLabel(customScrollLogicLabel);
        //cursor.Emit(OpCodes.Ldarg_0);
        //cursor.Emit(OpCodes.Ldarg_0);
        //cursor.Emit<MenuScrollBox>(OpCodes.Ldfld, "parentPage");
        //cursor.EmitDelegate((MenuScrollBox menuScrollBox, MenuPage menuPage) => {
        //    var yMovementInput = SemiFunc.InputMovementY() / 20f;
        //    var yMouseScroll = SemiFunc.InputScrollY();

        //    float amountToScroll;

        //    if (MenuAPI.customMenuPages.TryGetValue(menuPage, out var repoPopupPage) && repoPopupPage.scrollView.scrollSpeed is { } constantScrollSpeed)
        //    {
        //        constantScrollSpeed *= 10f;
                    
        //        var scrollableHeight = Mathf.Abs((float)REPOReflection.menuScrollBox_ScrollerEndPosition.GetValue(menuScrollBox) - (float)REPOReflection.menuScrollBox_ScrollerStartPosition.GetValue(menuScrollBox));
        //        amountToScroll = (yMovementInput + Math.Sign(yMouseScroll)) * constantScrollSpeed / scrollableHeight * menuScrollBox.scrollBarBackground.rect.height;
        //    }
        //    else
        //    {
        //        var scrollHeight = (float)REPOReflection.menuScrollBox_ScrollHeight.GetValue(menuScrollBox);
        //        amountToScroll = yMovementInput / (scrollHeight * 0.01f) + yMouseScroll / (scrollHeight * 0.01f);
        //    }
             
        //    var currentHandlePosition = (float)REPOReflection.menuScrollBox_ScrollHandleTargetPosition.GetValue(menuScrollBox);
        //    REPOReflection.menuScrollBox_ScrollHandleTargetPosition.SetValue(menuScrollBox, currentHandlePosition + amountToScroll);
        //});
             
        //cursor.GotoPrev(instruction => instruction.MatchCall(typeof(SemiFunc), "InputMovementY"));
             
        //var newLabel = cursor.MarkLabel();
        //cursor.Emit(OpCodes.Ldarg_0);
        //cursor.Emit<MenuScrollBox>(OpCodes.Ldfld, "scrollBoxActive");
        //cursor.Emit(OpCodes.Brfalse_S, postIfLabel);

        //cursor.GotoPrev(MoveType.After, instruction => instruction.MatchCall<Input>("GetMouseButton"));
        //cursor.Remove();
        //cursor.Emit(OpCodes.Brfalse, newLabel);
            
        //cursor.GotoNext(MoveType.After, instruction => instruction.MatchCall(typeof(SemiFunc), "UIMouseHover"));
        //cursor.Remove();
        //cursor.Emit(OpCodes.Brfalse, newLabel);

        //cursor.GotoNext(instruction => instruction.MatchStfld<MenuScrollBox>("scrollAmount"));
        //cursor.Index -= 13;
        //cursor.RemoveRange(14);
        //cursor.EmitDelegate((MenuScrollBox instance) =>
        //{
        //    float scrollAmount;
        //    if (MenuAPI.customMenuPages.ContainsKey((MenuPage)REPOReflection.menuScrollBox_ParentPage.GetValue(instance)))
        //        scrollAmount = (instance.scrollHandle.localPosition.y + instance.scrollHandle.sizeDelta.y / 2f) / instance.scrollBarBackground.rect.height;
        //    else
        //        scrollAmount = instance.scrollHandle.localPosition.y / instance.scrollBarBackground.rect.height * 1.1f;
                
        //    REPOReflection.menuScrollBox_ScrollAmount.SetValue(instance, scrollAmount);
        //});
    }
    
    private static void ChatManager_StateInactiveILHook(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.After, instruction => instruction.MatchStfld<ChatManager>("chatActive"));
        
        var label = cursor.DefineLabel();
        
        cursor.Emit<REPOInputStringSystem>(OpCodes.Ldsfld, "hasAnyFocus");
        cursor.Emit(OpCodes.Brfalse_S, label);
        cursor.Emit(OpCodes.Ret);

        cursor.MarkLabel(label);
    }
        
    private void Awake()
    {
        logger.LogDebug("Hooking `MenuPageMain.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageMain), "Start"), MenuPageMain_StartHook);
            
        logger.LogDebug("Hooking `MenuPageSettings.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageSettings), "Start"), MenuPageSettings_StartHook);
            
        logger.LogDebug("Hooking `MenuPageColor.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageColor), "Start"), MenuPageColor_StartHook);
            
        logger.LogDebug("Hooking `MenuPageEsc.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageEsc), "Start"), MenuPageEsc_StartHook);
        
        logger.LogDebug("Hooking `MenuPageRegions.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageRegions), "Start"), MenuPageRegions_StartHook);
        
        logger.LogDebug("Hooking `MenuPageServerList.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageServerList), "Start"), MenuPageServerList_StartHook);
        
        logger.LogDebug("Hooking `MenuPageLobby.Start`");
        new Hook(AccessTools.Method(typeof(MenuPageLobby), "Start"), MenuPageLobby_StartHook);
        
        logger.LogDebug("Hooking `SemiFunc.UIMouseHover`");
        new ILHook(AccessTools.Method(typeof(SemiFunc), "UIMouseHover"), SemiFunc_UIMouseHoverILHook);
            
        logger.LogDebug("Hooking `MenuPage.StateClosing`");
        new ILHook(AccessTools.Method(typeof(MenuPage), "StateClosing"), MenuPage_StateClosingILHook);
            
        logger.LogDebug("Hooking `MenuScrollBox.Update`");
        new ILHook(AccessTools.Method(typeof(MenuScrollBox), "Update"), MenuScrollBox_UpdateILHook);
        
        logger.LogDebug("Hooking `ChatManager.StateInactive`");
        new ILHook(AccessTools.Method(typeof(ChatManager), "StateInactive"), ChatManager_StateInactiveILHook);
    }
}