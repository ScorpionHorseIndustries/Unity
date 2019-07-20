function GetStockpileQty(inventoryItemName) 
	return World.current.inventoryManager:GetStockpileQty(inventoryItemName)
end

function GetStockPileQty(inventoryItemName)
	return GetStockpileQty(inventoryItemName)
end

function GetLooseQty(inventoryItemName)
	return World.current.inventoryManager:GetLooseQty(inventoryItemName)

end

function GetQty(inventoryItemName) 
	return World.current.inventoryManager:GetStockpileQty(inventoryItemName)
end

function GetMaxQty(inventoryItemName)
	return World.current.inventoryManager:GetStockpileMaxQty(inventoryItemName)
end

function GetTotalQty(inventoryItemName)
	return GetQty(inventoryItemName) + GetLooseQty(inventoryItemName)
end

csInventoryIndex = 0

function CheckStockpiles()

	
	local toCreate = {}

	local inventories = World.current.inventoryManager.stockpiles:GetEnumerator()
	--local totalInventories = 
	local currentinvIndex = -1
	while inventories:MoveNext() do
		currentinvIndex = currentinvIndex + 1
		if (currentinvIndex ~= csInventoryIndex) then
			goto nextInventory
		else
			
			--World.dbug("csInventoryIndex = " .. tostring(csInventoryIndex))
		end
		
		
		local inventory = inventories.Current
		if (inventory.IsStockPile) then 
			
			if (inventory.tile.HasPendingWork) then goto nextInventory end

			local spsList = World.current.inventoryManager.stockpileSettings.Values:GetEnumerator()

			while spsList:MoveNext() do
				local sps = spsList.Current
				if (sps.pendingWork == false and (inventory:TotalQty() == 0 or inventory:HowMany(sps.name) > 0)) then
					local tileQty = inventory:HowMany(sps.name)
					

					if (sps.currentQty < sps.maxQty and tileQty < sps.stackSize) then

						local looseQty = World.current.inventoryManager:GetLooseQty(sps.name)
						
						local qtyWanted = sps.maxQty - sps.currentQty
						if (qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize
						end

						if (tileQty + qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize - tileQty
						end

						if (qtyWanted > looseQty) then
							qtyWanted = looseQty
						end
						if (qtyWanted > 0) then
							toCreate[#toCreate+1] = {tile = inventory.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
							sps.pendingWork = true
							goto finish
						end
						--WorkItem.MakeWorkItem(inventory.tile,"SetHaul",sps.name,qtyWanted)


					end
				end

			end
		end
		::nextInventory::
	end
	::finish::

	for n = 1, #toCreate do
		a = toCreate[n]
		WorkItem.MakeWorkItem(a["tile"], a["func"], a["name"], a["qty"])

	end
	if (World.current.inventoryManager.stockpiles.Count > 0) then
		csInventoryIndex = (csInventoryIndex + 1 ) % World.current.inventoryManager.stockpiles.Count
	end


end