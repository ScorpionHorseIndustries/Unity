function OnInstalledItemJobComplete(job) 
    --Debug.Log("installed item job complete: " + job);
	job.tile:RemoveJob(job)
    
	if (World.current:PlaceInstalledItem(job.description, job.tile) == nil) then
		job.inventory:Explode()
	end

end

function OnRemoveInstalledItemJobComplete(job) 
	job.tile:RemoveJob(job)
    if (job.tile.installedItem ~= nil) then
      job.tile.installedItem:Deconstruct()
    end
end

function JobQueue_JobComplete(job)

end
function JobQueue_JobCancelled(job)
end


function JobQueue_HaulToTileComplete(job)
    job.tile:RemoveJob(job)
end

function JobQueue_HaulJobComplete(job)
	-- job.recipe.Add(job.recipeResourceName, job.qtyFulfilled);
    job.tile:RemoveJob(job)

end

function JobQueue_HaulJobCancelled(job)
	job.tile:RemoveJob(job)
end