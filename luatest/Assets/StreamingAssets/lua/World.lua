function GetStockpileTotal(inventoryItemName) 
	local invTotal = 0
	local inventories = World.current.inventoryManager.inventories:GetEnumerator()

	while inventories:MoveNext() do
		inventory = inventories.Current

		if (inventory ~= nil and inventory.tile ~= nil and inventory.tile.installedItem ~= nil and inventory.tile.installedItem.type == "installed::stockpile") then
			invTotal = invTotal + inventory:HowMany(inventoryItemName)
		end

	end
	return invTotal


end


function CheckStockpiles()
	local toCreate = {}
	local inventories = World.current.inventoryManager.inventories:GetEnumerator()

	while inventories:MoveNext() do
		
		local inventory = inventories.Current
		if (inventory.IsStockPile) then 
			
			if (inventory.tile.HasPendingWork) then goto nextInventory end

			local spsList = World.current.inventoryManager.stockpileSettings.Values:GetEnumerator()

			while spsList:MoveNext() do
				local sps = spsList.Current
				if ((inventory:TotalQty() == 0 or inventory:HowMany(sps.name) > 0) and sps.pendingWork == false) then

					if (sps.currentQty < sps.maxQty) then

						local qtyWanted = sps.maxQty - sps.currentQty
						if (qtyWanted > sps.stackSize) then
							qtyWanted = sps.stackSize
						end

						toCreate[#toCreate+1] = {tile = inventory.tile, func = "SetHaul", name = sps.name, qty = qtyWanted}
						sps.pendingWork = true
						goto nextInventory
						--WorkItem.MakeWorkItem(inventory.tile,"SetHaul",sps.name,qtyWanted)


					end
				end

			end
		end
		::nextInventory::
	end

	for n = 1, #toCreate do
		a = toCreate[n]
		WorkItem.MakeWorkItem(a["tile"], a["func"], a["name"], a["qty"])

	end


end