using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using TheCodeCamp.Data;
using TheCodeCamp.Models;

namespace TheCodeCamp.Controllers
{
    [RoutePrefix("api/camps/{moniker}/talks")]
    public class TalksController : ApiController
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;

        public TalksController(ICampRepository campRepository, IMapper mapper)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
        }

        [Route()]
        public async Task<IHttpActionResult> Get(string moniker, bool includeSpeakers = false)
        {
            try
            {
                var results = await campRepository.GetTalksByMonikerAsync(moniker, includeSpeakers);
                return Ok(mapper.Map<IEnumerable<TalkModel>>(results));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{id:int}", Name = "GetTalk")]
        public async Task<IHttpActionResult> Get(string moniker, int id, bool includeSpeakers = false)
        {
            try
            {
                var result = await campRepository.GetTalkByMonikerAsync(moniker, id, includeSpeakers);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(mapper.Map<TalkModel>(result));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route()]
        public async Task<IHttpActionResult> Post(string moniker, TalkModel talkModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var camp = await campRepository.GetCampAsync(moniker);
                    if (camp != null)
                    {
                        var talk = mapper.Map<Talk>(talkModel);
                        talk.Camp = camp;
                        campRepository.AddTalk(talk);

                        // Add speaker if provided
                        if (talkModel.Speaker != null)
                        {
                            var speaker = await campRepository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                            talk.Speaker = speaker;
                        }

                        if (await campRepository.SaveChangesAsync())
                        {
                            return CreatedAtRoute("GetTalk", new { moniker = moniker, id = talk.TalkId }, mapper.Map<TalkModel>(talk));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalServerError(ex);
            }
            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Put(string moniker, int talkId, TalkModel talkModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var talk = await campRepository.GetTalkByMonikerAsync(moniker, talkId, true);
                    if (talk == null)
                    {
                       return NotFound();
                    }
                    mapper.Map(talkModel, talk);
                    if (await campRepository.SaveChangesAsync())
                    {
                        return Ok(mapper.Map<TalkModel>(talk));
                    }
                }
            }
            catch (Exception ex)
            {
                InternalServerError(ex);
            }
            return BadRequest(ModelState);
        }

        [Route("{talkId:int}")]
        public async Task<IHttpActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null)
                {
                    return NotFound();
                }
                campRepository.DeleteTalk(talk);
                if (await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                InternalServerError(ex);
            }
            return InternalServerError();
        }
    }
}